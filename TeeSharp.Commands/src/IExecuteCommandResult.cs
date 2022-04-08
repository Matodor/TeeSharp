using System.Threading.Tasks;
using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands;

public interface IExecuteCommandResult
{
    /// <summary>
    ///
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    ///
    /// </summary>
    CommandArgs Args { get; }

    /// <summary>
    ///
    /// </summary>
    CommandContext Context { get; }

    /// <summary>
    ///
    /// </summary>
    LineParseError? LineParseError { get; }

    /// <summary>
    ///
    /// </summary>
    ArgumentsParseError? ArgumentsParseError { get; }

    /// <summary>
    ///
    /// </summary>
    ExecuteCommandError? Error { get; }

    Task? ExecuteTask { get; }
}
