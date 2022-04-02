using System;
using System.Collections.Generic;
using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands.Parsers;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class DefaultCommandArgumentsParser : ICommandArgumentsParser
{
    public virtual bool TryParse(
        string? input, 
        IReadOnlyList<IParameterInfo> parameters, 
        out CommandArgs? args, 
        out ArgumentsParseError? error)
    {
        if (parameters.Count == 0)
        {
            args = CommandArgs.Empty;
            error = null;
            return true;
        }

        input = input?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            if (!parameters[0].IsOptional)
            {
                args = null;
                error = ArgumentsParseError.MissingArgument;
                return false;
            }
                
            args = CommandArgs.Empty;
            error = null;
            return true;
        }

        var values = new List<object>(parameters.Count);
        var line = input.AsSpan();
            
        foreach (var parameter in parameters)
        {
            var arg = parameter.IsRemain 
                ? line.TrimStart().ToString()
                : GetFirstChunk(line, out line);

            if (string.IsNullOrEmpty(arg))
            {
                if (parameter.IsOptional) 
                    continue;
                    
                args = null;
                error = ArgumentsParseError.MissingArgument;
                return false;
            }

            if (arg[0] == '"' && (arg.Length == 1 || arg[^1] != '"' || arg[^2] == '\\') ||
                arg[^1] == '"' && (arg.Length == 1 || arg[^2] != '\\' && arg[0] != '"'))
            {
                args = null;
                error = ArgumentsParseError.MissingQuote;
                return false;
            }

            arg = arg.Replace("\\\"", "\"");
                
            if (parameter.ArgumentReader.TryRead(arg, out var value))
                values.Add(value);
            else
            {
                args = null;
                error = ArgumentsParseError.ReadArgumentFailed;
                return false;
            }
        }

        args = new CommandArgs(values);
        error = null;
        return true;
    }

    protected virtual string? GetFirstChunk(
        ReadOnlySpan<char> line, 
        out ReadOnlySpan<char> restLine)
    {
        if (line == null || line.IsEmpty)
        {
            restLine = null;
            return null;
        }
            
        var isQuoteable = line.Length > 1 && line[0] == '"';
        if (isQuoteable)
        {
            var firstSpaceIndex = -1;
                
            for (var i = 1; i < line.Length; i++)
            {
                if (firstSpaceIndex == -1 && char.IsWhiteSpace(line[i]))
                {
                    firstSpaceIndex = i - 1;
                    continue;
                }

                // ReSharper disable once InvertIf
                if (line[i] == '"'
                    && line[i - 1] != '\\'
                    && (i + 1 == line.Length || line[i + 1] == ' '))
                {
                    restLine = line.Slice(i + 1).TrimStart();
                    return line.Slice(1, i - 1).ToString();
                }
            }

            if (firstSpaceIndex == -1)
            {
                restLine = null;
                return line.ToString();
            }

            restLine = line.Slice(firstSpaceIndex).TrimStart();
            return line.Slice(0, firstSpaceIndex).ToString();
        }
            
        var spaceIndex = line.IndexOf(' ');
        if (spaceIndex == -1)
        {
            restLine = null;
            return line.ToString();
        }
            
        restLine = line.Slice(spaceIndex).TrimStart();
        return line.Slice(0, spaceIndex).ToString();
    }
}