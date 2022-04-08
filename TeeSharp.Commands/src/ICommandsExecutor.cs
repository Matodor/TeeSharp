using System;
using System.Threading;

namespace TeeSharp.Commands;

public interface ICommandsExecutor
{
    ICommandsDictionary Commands { get; }
    ICommandLineParser LineParser { get; }
    ICommandArgumentsParser ArgumentsParser { get; }

    public IExecuteCommandResult Execute(
        ReadOnlySpan<char> line,
        CommandContext context,
        CancellationToken cancellationToken);
}
