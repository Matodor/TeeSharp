using System;
using TeeSharp.Commands.Parsers;

namespace TeeSharp.Commands;

public class CommandsExecutor : ICommandsExecutor
{
    public virtual ICommandsDictionary Commands { get; protected set; } = null!;
    public virtual ICommandLineParser LineParser { get; protected set; } = null!;
    public virtual ICommandArgumentsParser ArgumentsParser { get; protected set; } = null!;

    public virtual void Init()
    {
        Commands = new CommandsDictionary();
        LineParser = new DefaultCommandLineParser();
        ArgumentsParser = new DefaultCommandArgumentsParser();
    }

    public virtual ICommandResult Execute(string line)
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
