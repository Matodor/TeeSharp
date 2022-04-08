using System;
using System.Diagnostics.CodeAnalysis;
using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands.Parsers;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class DefaultCommandLineParser : ICommandLineParser
{
    public string Prefix
    {
        get => _prefix;
        set
        {
            _prefix = string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Trim();
        }
    }

    private string _prefix = string.Empty;

    public DefaultCommandLineParser(string prefix = "/")
    {
        Prefix = prefix;
    }

    public virtual bool TryParse(
        ReadOnlySpan<char> line,
        out ReadOnlySpan<char> command,
        out ReadOnlySpan<char> args,
        [NotNullWhen(false)] out LineParseError? parseError)
    {
        line = line.Trim();

        if (!Valid(line, out var spaceIndex, out parseError))
        {
            command = default;
            args = default;
            return false;
        }

        command = spaceIndex != -1
            ? line.Slice(Prefix.Length, spaceIndex - Prefix.Length)
            : line.Slice(Prefix.Length);

        args = spaceIndex != -1
            ? line.Slice(spaceIndex + 1)
            : default;

        return true;
    }

    protected virtual bool Valid(ReadOnlySpan<char> line,
        out int spaceIndex,
        [NotNullWhen(false)] out LineParseError? error)
    {
        if (line.IsEmpty)
        {
            error = LineParseError.EmptyLine;
            spaceIndex = -1;
            return false;
        }

        if (line.Length < Prefix.Length + CommandInfo.MinCommandLength)
        {
            error = LineParseError.BadLength;
            spaceIndex = -1;
            return false;
        }

        if (Prefix.Length > 0 && !line.StartsWith(Prefix))
        {
            error = LineParseError.WrongPrefix;
            spaceIndex = -1;
            return false;
        }

        spaceIndex = line.IndexOf(' ');

        if (spaceIndex != -1 &&
            spaceIndex < Prefix.Length + CommandInfo.MinCommandLength)
        {
            error = LineParseError.BadLength;
            spaceIndex = -1;
            return false;
        }

        error = null;
        return true;
    }
}
