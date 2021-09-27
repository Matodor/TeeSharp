using TeeSharp.Common.Commands.Parsers;

namespace TeeSharp.Common.Commands
{
    public abstract class BaseCommandsExecutor
    {
        public BaseCommandsDictionary Commands { get; protected set; }
        
        protected ICommandLineParser LineParser { get; set; }
        protected ICommandArgumentsParser ArgumentsParser { get; set; }

        public abstract void Init();
        public abstract ICommandResult Execute(string line);
    }
}