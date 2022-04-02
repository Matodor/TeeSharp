using System;
using TeeSharp.Commands.Parsers;

namespace TeeSharp.Commands;

public class CommandsExecutor : BaseCommandsExecutor
{
    public override void Init()
    {
        Commands = new CommandsDictionary();
        LineParser = new DefaultCommandLineParser();
        ArgumentsParser = new DefaultCommandArgumentsParser();
    }

    public override ICommandResult Execute(string line)
    {
        throw new NotImplementedException();
        // if (!LineParser.TryParse(line, out var command, out var args, out var error))
        // {
        //     return new CommandResult(
        //         args: CommandArgs.Empty,
        //         error: CommandResultError.ParseFailed
        //     );
        // }
        //
        // if (!Commands.ContainsKey(command))
        // {
        //     return new CommandResult(
        //         args: CommandArgs.Empty,
        //         error: CommandResultError.CommandNotFound
        //     );
        // }
    }
}