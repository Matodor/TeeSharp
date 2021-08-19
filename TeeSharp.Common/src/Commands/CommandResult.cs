namespace TeeSharp.Common.Commands
{
    public class CommandResult : ICommandResult
    {
        public CommandArgs Args { get; }
        public bool IsSuccess { get; }
        public CommandResultError Error { get; }
        
        public CommandResult(CommandArgs args, CommandResultError error, bool isSuccess)
        {
            Args = args;
            IsSuccess = isSuccess;
            Error = error;
        }
    }
}