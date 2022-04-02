using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands.Parsers;

public interface ICommandLineParser
{
    public bool TryParse(string line, out string command, out string args, 
        out LineParseError? parseError);
}