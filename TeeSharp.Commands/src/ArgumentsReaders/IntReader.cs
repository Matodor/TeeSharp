using System;

namespace TeeSharp.Commands.ArgumentsReaders;

public class IntReader : IArgumentReader
{
    public bool TryRead(ReadOnlySpan<char> arg, out object value)
    {
        var result = int.TryParse(arg, out var @int);
        value = @int;
        return result;
    }
}
