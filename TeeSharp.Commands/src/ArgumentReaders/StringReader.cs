using System;

namespace TeeSharp.Commands.ArgumentReaders;

public class StringReader : IArgumentReader
{
    public bool TryRead(ReadOnlySpan<char> arg, out object value)
    {
        value = arg.ToString();
        return true;
    }
}
