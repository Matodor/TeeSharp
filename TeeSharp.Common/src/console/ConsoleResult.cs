using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeeSharp.Common.Console
{
    public class ConsoleResult
    {
        public int NumArguments => _arguments.Count;
        public object this[int index] => _arguments[index];

        private readonly string _argsStored;
        private readonly IList<object> _arguments;

        public ConsoleResult(string args)
        {
            _argsStored = args.Trim();
            _arguments = new List<object>();
        }
        
        public bool ParseArguments(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
                return true;

            if (format.Any(c => !ConsoleCommand.ARGUMENTS_TYPES.Contains(c)))
                return false;

            var args = new List<string>();
            var builder = new StringBuilder();
            var quoteIndex = -1;

            for (var i = 0; i < _argsStored.Length; i++)
            {
                if (_argsStored[i] == '"')
                {
                    if (quoteIndex < 0)
                        quoteIndex = i;
                    else
                        quoteIndex = -1;
                    continue;
                }

                if (_argsStored[i] == '"' && quoteIndex > 0 ||
                    _argsStored[i] == ' ' && quoteIndex < 0)
                {
                    quoteIndex = -1;
                    args.Add(builder.ToString());
                    builder.Clear();
                    continue;
                }

                builder.Append(_argsStored[i]);
            }

            if (builder.Length > 0)
                args.Add(builder.ToString());

            var optional = false;
            var countNonOptional = 0;
            var index = 0;

            for (var i = 0; i < format.Length; i++)
            {
                if (format[i] == '?')
                {
                    optional = true;
                    continue;
                }

                if (optional)
                    optional = false;
                else
                    countNonOptional++;

                if (countNonOptional > args.Count)
                    return false;

                if (format[i] == 's')
                {
                    _arguments.Add(args[index]);
                }
                else if (format[i] == 'i')
                {
                    if (int.TryParse(args[index], out var result))
                        _arguments.Add(result);
                    else
                        return false;
                }
                else if (format[i] == 'f')
                {
                    if (float.TryParse(args[index], out var result))
                        _arguments.Add(result);
                    else
                        return false;
                }

                index++;
            }

            return true;
        }
    }
}