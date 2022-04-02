using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands
{
    public class CommandResult : ICommandResult
    {
        public CommandArgs Args { get; }
        public CommandContext Context { get; }
        public CommandResultError? Error { get; }

        public bool IsSuccess => !Error.HasValue;
        
        public CommandResult(
            CommandArgs args, 
            CommandContext context, 
            CommandResultError? error)
        {
            Args = args;
            Context = context;
            Error = error;
        }
    }
}
