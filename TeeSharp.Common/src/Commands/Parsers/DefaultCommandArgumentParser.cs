using System;
using System.Collections.Generic;
using System.Linq;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Common.Commands.Parsers
{
    public class DefaultCommandArgumentParser : ICommandArgumentParser
    {
        private const char ParameterString = 's';
        private const char ParameterFloat = 'f';
        private const char ParameterInt = 'i';
        private const char ParameterRest = 'r';
        private const char ParameterOptional = '?';

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public IEnumerable<object> Parse(string arguments, string pattern)
        {
            if (!Valid(pattern))
            {
                return null;
            }

            // no pattern
            if (string.IsNullOrEmpty(pattern))
            {
                return new object[] {arguments};
            }

            return Algo(arguments, pattern);
        }

        private static IEnumerable<object> Algo(string arguments, string pattern)
        {
            var parameterIndex = 0;
            var optional = false;
            var list = new List<object>();
            ReadOnlySpan<char> argsSpan = arguments;

            while (true)
            {
                if (parameterIndex >= pattern.Length)
                    break;

                var parameterType = pattern[parameterIndex++];
                if (parameterType == ParameterOptional)
                {
                    optional = true;
                    continue;
                }

                argsSpan = argsSpan.SkipWhitespaces();

                if (argsSpan.Length == 0)
                {
                    if (!optional)
                    {
                        return null;
                    }

                    break;
                }

                if (argsSpan[0] == '"')
                {
                    argsSpan = argsSpan[1..];

                    for (var i = 0; i < argsSpan.Length; i++)
                    {
                        if (argsSpan[i] == '"')
                        {
                            list.Add(argsSpan[..i].ToString());
                            argsSpan = argsSpan[(i + 1)..];
                            break;
                        }

                        if (argsSpan[i] == '\\')
                        {
                            if (i + 1 < argsSpan.Length && (argsSpan[i + 1] == '\\' || argsSpan[i + 1] == '"'))
                                i++;
                        }
                        else if (i + 1 == argsSpan.Length)
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    if (parameterType == ParameterRest)
                    {
                        list.Add(argsSpan.ToString());
                        break;
                    }

                    var temp = argsSpan.SkipToWhitespaces();
                    var str = argsSpan[..^temp.Length];

                    switch (parameterType)
                    {
                        case ParameterInt when int.TryParse(str, out var @int):
                            list.Add(@int);
                            break;
                        case ParameterFloat when float.TryParse(str, out var @float):
                            list.Add(@float);
                            break;
                        case ParameterString:
                            list.Add(str.ToString());
                            break;
                    }

                    argsSpan = temp;
                }
            }

            return list;
        }

        private static bool Valid(string pattern)
        {
            return
                //valid pattern
                !pattern.Any(parameterType =>
                    parameterType != ParameterString &&
                    parameterType != ParameterFloat &&
                    parameterType != ParameterInt &&
                    parameterType != ParameterRest &&
                    parameterType != ParameterOptional)
                // rest argument must be in end of the pattern
                && (!pattern.Contains('r') || pattern.IndexOf('r') == pattern.Length - 1);
        }
    }
}