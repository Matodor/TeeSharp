using System;
using System.Collections.Generic;
using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands.Parsers
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class DefaultCommandArgumentsParser : ICommandArgumentsParser
    {
        public virtual bool TryParse(
            string input, 
            IReadOnlyList<IParameterInfo> parameters, 
            out CommandArgs args, 
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
                var arg = GetFirstArgument(line, out line);

                if (string.IsNullOrEmpty(arg))
                {
                    if (!parameter.IsOptional)
                    {
                        args = null;
                        error = ArgumentsParseError.MissingArgument;
                        return false;
                    }
                }
                else
                {
                    if (parameter.ArgumentReader.TryRead(arg, out var value))
                        values.Add(value);
                    else
                    {
                        args = null;
                        error = ArgumentsParseError.ReadArgumentFailed;
                        return false;
                    }
                }
            }

            args = new CommandArgs(values);
            error = null;
            return true;
        }

        protected virtual string? GetFirstArgument(
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
                var endIndex = 0;
                
                for (var i = 1; i < line.Length; i++)
                {
                    if (char.IsWhiteSpace(line[i]) ||
                        line[i] == '"' && line[i - 1] != '\\')
                    {
                        endIndex = i - 1;
                        break;
                    }

                    endIndex = i;
                }

                restLine = line.Slice(endIndex).TrimStart();
                return line.Slice(1, endIndex - 1).ToString();
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
}
