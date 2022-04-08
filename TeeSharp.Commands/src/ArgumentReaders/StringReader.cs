using System;
using System.Diagnostics.CodeAnalysis;

namespace TeeSharp.Commands.ArgumentReaders;

public class StringReader : IArgumentReader
{
    public bool TryRead(ReadOnlySpan<char> arg, [MaybeNullWhen(false)] out object value)
    {
        value = arg.ToString();
        return true;
    }
}
