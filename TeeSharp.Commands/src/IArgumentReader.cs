using System;
using System.Diagnostics.CodeAnalysis;

namespace TeeSharp.Commands;

public interface IArgumentReader
{
    public bool TryRead(ReadOnlySpan<char> arg, [MaybeNullWhen(false)] out object value);
}
