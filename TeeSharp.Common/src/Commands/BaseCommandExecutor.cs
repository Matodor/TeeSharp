using System;
using Serilog;
using TeeSharp.Common.Commands.Parsers;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Common.Commands
{
    public class BaseCommandExecutor : IContainerService
    {
        private readonly ICommandArgumentParser _parser = new DefaultCommandArgumentParser();
        public CommandsDictionary Commands { get; set; }
        public Container Container { get; set; }

        public void Init()
        {
            Commands = Container.Resolve<CommandsDictionary>();
        }

        public bool ExecuteLine(string line, int accessLevel)
        {
            if (string.IsNullOrEmpty(line))
                return false;

            var (ok, command, args) = Commands.Parse(line);

            if (!ok)
            {
                Log.Information($"[command executor] No such command for line: {line}");
                return false;
            }

            var arguments = _parser.Parse(args, command.Pattern);

            if (arguments == null)
            {
                Log.Information($"[command executor] Invalid arguments... Usage: {args} {command.Pattern}");
            }

            if (accessLevel == -1 || accessLevel >= command.AccessLevel)
                command.Invoke(arguments);
            else
            {
                Log.Information($"[command executor] Insufficient access level for execute command '{line}'");
                return false;
            }

            return true;
        }
    }
}