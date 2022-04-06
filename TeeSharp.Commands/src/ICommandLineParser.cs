using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands;

public interface ICommandLineParser
{
    string Prefix { get; set; }

    bool TryParse(
        string? line,
        out string? command,
        out string? args,
        out LineParseError? parseError);
}
