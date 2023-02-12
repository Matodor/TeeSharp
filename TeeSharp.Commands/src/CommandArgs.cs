using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TeeSharp.Commands;

public class CommandArgs :
    IReadOnlyDictionary<string, object>,
    IEquatable<CommandArgs>
{
    public static readonly CommandArgs Empty = new(new Dictionary<string, object>());

    public int Count => Arguments.Count;
    public object this[string key] => Arguments[key];
    public IEnumerable<string> Keys => Arguments.Keys;
    public IEnumerable<object> Values => Arguments.Values;

    protected IReadOnlyDictionary<string, object> Arguments { get; }

    public CommandArgs(IReadOnlyDictionary<string, object> args)
    {
        Arguments = args;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return Arguments.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Arguments).GetEnumerator();
    }

    public bool ContainsKey(string key)
    {
        return Arguments.ContainsKey(key);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
    {
        return Arguments.TryGetValue(key, out value);
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public bool Equals(CommandArgs? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return
            Arguments.Count == other.Count &&
            Arguments.All(kv =>
                other.TryGetValue(kv.Key, out var otherValue) && (
                    kv.Value == otherValue ||
                    // TODO useless?
                    // kv.Value is float f1 && otherValue is float f2 && string.Equals(
                    //     f1.ToString(CultureInfo.CurrentCulture),
                    //     f2.ToString(CultureInfo.CurrentCulture)
                    // ) ||
                    kv.Value.Equals(otherValue)
                )
            );
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((CommandArgs)obj);
    }

    public override int GetHashCode()
    {
        return Arguments.GetHashCode();
    }
}
