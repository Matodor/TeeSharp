using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Common.Console
{
    public class ConsoleCommandResult
    {
        public int ArgumentsCount => _arguments.Length;
        public object this[int index] => _arguments[index];

        private readonly object[] _arguments;

        private ConsoleCommandResult(object[] arguments)
        {
            _arguments = arguments;
        }

        public static bool Parse(in string arguments, in string parameters, out ConsoleCommandResult result)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                result = new ConsoleCommandResult(new object[] {arguments});
                return true;
            }

            if (parameters.Any(parameterType => 
                parameterType != ConsoleCommand.ParameterString &&
                parameterType != ConsoleCommand.ParameterFloat &&
                parameterType != ConsoleCommand.ParameterInt &&
                parameterType != ConsoleCommand.ParameterRest &&
                parameterType != ConsoleCommand.ParameterOptional))
            {
                result = null;
                return false;
            }

            var parameterIndex = 0;
            var optional = false;
            var list = new List<object>();
            ReadOnlySpan<char> argsSpan = arguments;

            while (true)
            {
                if (parameterIndex >= parameters.Length)
                    break;

                var parameterType = parameters[parameterIndex++];
                if (parameterType == ConsoleCommand.ParameterOptional)
                {
                    optional = true;
                    continue;
                }

                argsSpan = argsSpan.SkipWhitespaces();

                if (argsSpan.Length == 0)
                {
                    if (!optional)
                    {
                        result = null;
                        return false;
                    }

                    break;
                }

                if (argsSpan[0] == '"')
                {
                    argsSpan = argsSpan.Slice(1);

                    for (var i = 0; i < argsSpan.Length; i++)
                    {
                        if (argsSpan[i] == '"')
                        {
                            list.Add(argsSpan.Slice(0, i).ToString());
                            argsSpan = argsSpan.Slice(i + 1);
                            break;
                        }

                        if (argsSpan[i] == '\\')
                        {
                            if (i + 1 < argsSpan.Length && (argsSpan[i + 1] == '\\' || argsSpan[i + 1] == '"'))
                                i++;
                        }
                        else if (i + 1 == argsSpan.Length)
                        {
                            result = null;
                            return false;
                        }
                    }
                }
                else
                {
                    if (parameterType == ConsoleCommand.ParameterRest)
                    {
                        list.Add(argsSpan.ToString());
                        break;
                    }

                    var temp = argsSpan.SkipToWhitespaces();
                    var str = argsSpan.Slice(0, argsSpan.Length - temp.Length);

                    switch (parameterType)
                    {
                        case ConsoleCommand.ParameterInt when int.TryParse(str, out var @int):
                            list.Add(@int);
                            break;
                        case ConsoleCommand.ParameterFloat when float.TryParse(str, out var @float):
                            list.Add(@float);
                            break;
                        case ConsoleCommand.ParameterString:
                            list.Add(str.ToString());
                            break;
                    }

                    argsSpan = temp;
                }
            }

            result = new ConsoleCommandResult(list.ToArray());
            return true;
        }
    }
}