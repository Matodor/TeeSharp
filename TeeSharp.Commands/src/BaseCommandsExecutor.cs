using TeeSharp.Commands.Parsers;

namespace TeeSharp.Commands
{
    public abstract class BaseCommandsExecutor
    {
        public BaseCommandsDictionary Commands { get; protected set; } = null!;
        
        protected ICommandLineParser LineParser { get; set; } = null!;
        protected ICommandArgumentsParser ArgumentsParser { get; set; } = null!;

        public abstract void Init();
        public abstract ICommandResult Execute(string line);
    }
}
