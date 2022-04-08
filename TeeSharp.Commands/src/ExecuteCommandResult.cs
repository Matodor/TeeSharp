using System.Threading.Tasks;
using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands;

public class ExecuteCommandResult : IExecuteCommandResult
{
    public bool IsSuccess => !Error.HasValue;

    public CommandArgs Args { get; }
    public CommandContext Context { get; }
    public ArgumentsParseError? ArgumentsParseError { get; }
    public LineParseError? LineParseError { get; }
    public ExecuteCommandError? Error { get; }
    public Task? ExecuteTask { get; }

    public ExecuteCommandResult(
        CommandArgs args,
        CommandContext context,
        LineParseError? lineParseError = null,
        ArgumentsParseError? argumentsParseError = null,
        ExecuteCommandError? error = null,
        Task? executeTask = null)
    {
        Args = args;
        Context = context;
        LineParseError = lineParseError;
        ArgumentsParseError = argumentsParseError;
        Error = error;
        ExecuteTask = executeTask;
    }
}
