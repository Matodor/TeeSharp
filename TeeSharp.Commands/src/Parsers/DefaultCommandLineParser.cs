using System;
using System.Diagnostics;
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
        string? line,
        out string? command,
        out string? args,
        out LineParseError? parseError)
    {
        line = line?.Trim();

        if (!Valid(line, out var spaceIndex, out parseError))
        {
            command = null;
            args = null;
            return false;
        }

        if (line == null)
            throw new NullReferenceException(nameof(line));

        command = spaceIndex < 0
            ? Prefix.Length == 0
                ? line
                : line.Substring(Prefix.Length)
            : Prefix.Length == 0
                ? line.Substring(0, spaceIndex)
                : line.Substring(Prefix.Length, spaceIndex - Prefix.Length);

        args = spaceIndex < 0
            ? null
            : line.Substring(spaceIndex + 1);

        return true;
    }

    protected virtual bool Valid(
        string? line,
        out int spaceIndex,
        out LineParseError? error)
    {
        if (string.IsNullOrEmpty(line))
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

        if (Prefix.Length != 0 && !line.StartsWith(Prefix))
        {
            error = LineParseError.WrongPrefix;
            spaceIndex = -1;
            return false;
        }

        spaceIndex = line.IndexOf(' ', Prefix.Length);

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
