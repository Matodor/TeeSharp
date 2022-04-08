using System;
using System.Threading;
using TeeSharp.Commands.Errors;
using TeeSharp.Commands.Parsers;

namespace TeeSharp.Commands;

public class CommandsExecutor : ICommandsExecutor
{
    public virtual ICommandsDictionary Commands { get; protected set; }
    public virtual ICommandLineParser LineParser { get; protected set; }
    public virtual ICommandArgumentsParser ArgumentsParser { get; protected set; }

    public CommandsExecutor()
    {
        Commands = new CommandsDictionary();
        LineParser = new DefaultCommandLineParser();
        ArgumentsParser = new DefaultCommandArgumentsParser();
    }

    public virtual IExecuteCommandResult Execute(
        ReadOnlySpan<char> line,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        if (!LineParser.TryParse(line,
            out var strCommand,
            out var strArgs,
            out var lineParseError))
        {
            return new ExecuteCommandResult(
                args: CommandArgs.Empty,
                context: context,
                executeTask: null,
                lineParseError: lineParseError,
                argumentsParseError: null,
                error: ExecuteCommandError.ParseFailed
            );
        }

        if (!Commands.TryGetValue(strCommand.ToString(), out var command))
        {
            return new ExecuteCommandResult(
                args: CommandArgs.Empty,
                context: context,
                executeTask: null,
                lineParseError: lineParseError,
                argumentsParseError: null,
                error: ExecuteCommandError.CommandNotFound
            );

        }

        if (!ArgumentsParser.TryParse(strArgs, command.Parameters,
            out var args,
            out var argumentsParseError))
        {
            return new ExecuteCommandResult(
                args: args,
                context: context,
                executeTask: null,
                lineParseError: lineParseError,
                argumentsParseError: argumentsParseError,
                error: argumentsParseError switch {
                    ArgumentsParseError.MissingArgument => ExecuteCommandError.BadArgumentsCount,
                    ArgumentsParseError.MissingQuote => ExecuteCommandError.ParseFailed,
                    ArgumentsParseError.ReadArgumentFailed => ExecuteCommandError.ParseFailed,
                    _ => throw new ArgumentOutOfRangeException(nameof(argumentsParseError)),
                }
            );
        }

        var executeTask = command.Callback(args, context, cancellationToken);
        return new ExecuteCommandResult(
            args: args,
            context: context,
            executeTask: executeTask
        );
    }
}
