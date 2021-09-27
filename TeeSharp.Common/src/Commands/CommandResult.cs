using TeeSharp.Common.Commands.Errors;

namespace TeeSharp.Common.Commands
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