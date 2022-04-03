using System;

namespace TeeSharp.Commands.ArgumentsReaders;

public class FloatReader : IArgumentReader
{
    public bool TryRead(ReadOnlySpan<char> arg, out object value)
    {
        var result = float.TryParse(arg, out var @float);
        value = @float;
        return result;
    }
}
