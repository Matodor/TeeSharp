using TeeSharp.Commands;

namespace TeeSharp.Commands;

public interface ICommandsExecutor
{
    ICommandsDictionary Commands { get; }
    ICommandLineParser LineParser { get; }
    ICommandArgumentsParser ArgumentsParser { get; }

    public void Init();
    public ICommandResult Execute(string line);
}
