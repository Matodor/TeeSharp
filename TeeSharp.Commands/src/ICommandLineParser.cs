using System;
using System.Diagnostics.CodeAnalysis;
using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands;

public interface ICommandLineParser
{
    string Prefix { get; set; }

    bool TryParse(
        ReadOnlySpan<char> line,
        out ReadOnlySpan<char> command,
        out ReadOnlySpan<char> args,
        [NotNullWhen(false)] out LineParseError? parseError);
}
