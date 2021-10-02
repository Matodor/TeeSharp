using System;
using System.Collections.Generic;
using TeeSharp.Common.Commands.Errors;

namespace TeeSharp.Common.Commands.Parsers
{
    public class DefaultCommandArgumentsParser : ICommandArgumentsParser
    {
        public bool TryParse(string input, IReadOnlyList<ParameterInfo> parameters, 
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

            if (string.IsNullOrEmpty(input) && !parameters[0].IsOptional)
            {
                args = null;
                error = ArgumentsParseError.MissingArgument;
                return false;
            }

            var line = input.AsSpan();
            for (var paramIndex = 0; paramIndex < parameters.Count; paramIndex++)
            {
                var result = GetFirstArgument(
                    line, 
                    parameters[paramIndex],
                    out line,
                    out var arg,
                    out error
                );

                if (error != null)
                {
                    args = null;
                    return false;
                }

                if (!result)
                {
                    if (paramIndex + 1 < parameters.Count && !parameters[paramIndex + 1].IsOptional)
                    {
                        args = null;
                        error = ArgumentsParseError.MissingArgument;
                        return false;
                    }
                    
                    break;
                }
            }

            throw new NotImplementedException();
        }

        protected bool GetFirstArgument(ReadOnlySpan<char> line, ParameterInfo parameterInfo,
            out ReadOnlySpan<char> restLine,
            out string arg,
            out ArgumentsParseError? error)
        {
            if (line == null || line.IsEmpty)
            {
                restLine = null;
                arg = null;
                error = parameterInfo.IsOptional 
                    ? (ArgumentsParseError?) null 
                    : ArgumentsParseError.MissingArgument;
                
                return false;
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

                arg = line.Slice(1, endIndex - 1).ToString();
                restLine = line.Slice(endIndex).TrimStart();
                error = null;

                return true;
            }
            
            var spaceIndex = line.IndexOf(' ');
            if (spaceIndex == -1)
            {
                arg = line.ToString();
                restLine = null;
                error = null;

                return false;
            }
            
            arg = line.Slice(0, spaceIndex).ToString();
            restLine = line.Slice(spaceIndex).TrimStart();
            error = null;

            return true;
        }
    }
}
