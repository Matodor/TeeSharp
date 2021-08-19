namespace TeeSharp.Common.Commands
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
        bool IsSuccess { get; }
        
        /// <summary>
        /// 
        /// </summary>
        CommandResultError Error { get; }
    }
}