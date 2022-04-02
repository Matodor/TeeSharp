using System;
using System.Collections;
using System.Collections.Generic;
using TeeSharp.Commands.Builders;

namespace TeeSharp.Commands;

public abstract class BaseCommandsDictionary : IDictionary<string, CommandInfo>
{
    public abstract event Action<string, CommandInfo>? CommandAdded;
    public abstract event Action<string>? CommandRemoved;
        
    public abstract CommandInfo this[string key] { get; set; }
    public abstract ICollection<string> Keys { get; }
    public abstract ICollection<CommandInfo> Values { get; }
    public abstract int Count { get; }
    public abstract bool IsReadOnly { get; }

    public abstract void Init();
    public abstract void Add(Action<CommandBuilder> factory);
    public abstract void Add(string key, CommandInfo commandInfo);
    public abstract void Add(KeyValuePair<string, CommandInfo> item);
    public abstract bool TryGetValue(string key, out CommandInfo value);
    public abstract bool Contains(KeyValuePair<string, CommandInfo> item);
    public abstract bool ContainsKey(string key);
    public abstract void CopyTo(KeyValuePair<string, CommandInfo>[] array, int arrayIndex);
    public abstract bool Remove(KeyValuePair<string, CommandInfo> item);
    public abstract bool Remove(string key);
    public abstract void Clear();
    public abstract IEnumerator<KeyValuePair<string, CommandInfo>> GetEnumerator();
        
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
