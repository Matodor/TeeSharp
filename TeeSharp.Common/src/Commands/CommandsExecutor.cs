using System;
using Serilog;
using TeeSharp.Common.Commands.Parsers;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Common.Commands
{
    public class CommandsExecutor : BaseCommandsExecutor
    {
        // public bool ExecuteLine(string line, int accessLevel)
        // {
        //     if (string.IsNullOrEmpty(line))
        //         return false;
        //
        //     var (ok, command, args) = Commands.Parse(line);
        //
        //     if (!ok)
        //     {
        //         Log.Information($"[command executor] No such command for line: {line}");
        //         return false;
        //     }
        //
        //     var arguments = _parser.Parse(args, command.ParametersPattern);
        //
        //     if (arguments == null)
        //     {
        //         Log.Information($"[command executor] Invalid arguments... Usage: {args} {command.ParametersPattern}");
        //     }
        //
        //     if (accessLevel == -1 || accessLevel >= command.AccessLevel)
        //         command.OnCommandExecuted(arguments);
        //     else
        //     {
        //         Log.Information($"[command executor] Insufficient access level for execute command '{line}'");
        //         return false;
        //     }
        //
        //     return true;
        // }

        public override void Init()
        {
            Commands = new CommandsDictionary();
            LineParser = new DefaultCommandLineParser();
            ArgumentsParser = new DefaultCommandArgumentsParser();
        }

        public override bool Execute(string line)
        {
            if (LineParser.TryParse(line, out var command, out var args, out var error))
            {
                
            }
        }
    }
}