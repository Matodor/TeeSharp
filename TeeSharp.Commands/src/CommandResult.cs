using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands
{
    public class CommandResult : ICommandResult
    {
        public CommandArgs Args { get; }
        
        public CommandContext Context { get; }

        public bool IsSuccess => !Error.HasValue;

        public CommandResultError? Error { get; }
        
        public CommandResult(CommandArgs args, CommandContext context, CommandResultError? error)
        {
            Args = args;
            Error = error;
        }
    }
}