using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands
{
    public interface ICommandResult
    {
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
        bool IsSuccess { get; }
        
        /// <summary>
        /// 
        /// </summary>
        CommandResultError? Error { get; }
    }
}