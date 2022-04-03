using System;

namespace TeeSharp.Commands;

public interface IArgumentReader
{
    public bool TryRead(ReadOnlySpan<char> arg, out object value);
}
