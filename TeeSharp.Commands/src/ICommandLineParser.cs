using System;
using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands;

public interface ICommandLineParser
{
    string Prefix { get; set; }

    bool TryParse(
        ReadOnlySpan<char> line,
        out ReadOnlySpan<char> command,
        out ReadOnlySpan<char> args,
        out LineParseError? parseError);
}
