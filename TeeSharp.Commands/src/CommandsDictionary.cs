using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Serilog;
using TeeSharp.Commands.Builders;

namespace TeeSharp.Commands;

/// <summary>
/// Commands store
/// </summary>
public class CommandsDictionary : ICommandsDictionary
{
    public event Action<string, CommandInfo>? CommandAdded;
    public event Action<string>? CommandRemoved;

    public virtual CommandInfo this[string key]
    {
        get => Dictionary[key];
        set
        {
            if (ContainsKey(key))
            {
                Dictionary[key] = value;
            }
            else
            {
                Add(key, value);
            }
        }
    }

    public virtual ICollection<string> Keys => Dictionary.Keys;

    public virtual ICollection<CommandInfo> Values => Dictionary.Values;

    public virtual int Count => Dictionary.Count;

    public virtual bool IsReadOnly => Dictionary.IsReadOnly;

    protected virtual IDictionary<string, CommandInfo> Dictionary { get; set; } = null!;

    public virtual void Init()
    {
        Dictionary = new Dictionary<string, CommandInfo>();
    }

    public virtual void Clear()
    {
        Dictionary.Clear();
    }

    public virtual IEnumerator<KeyValuePair<string, CommandInfo>> GetEnumerator()
    {
        return Dictionary.GetEnumerator();
    }

    public virtual bool TryGetValue(string key, [MaybeNullWhen(false)] out CommandInfo value)
    {
        return Dictionary.TryGetValue(key, out value);
    }

    public virtual bool Contains(KeyValuePair<string, CommandInfo> item)
    {
        return Dictionary.Contains(item);
    }

    public virtual bool ContainsKey(string key)
    {
        return Dictionary.ContainsKey(key);
    }

    public virtual void CopyTo(KeyValuePair<string, CommandInfo>[] array, int arrayIndex)
    {
        Dictionary.CopyTo(array, arrayIndex);
    }

    public virtual bool Remove(KeyValuePair<string, CommandInfo> item)
    {
        return Remove(item.Key);
    }

    public virtual bool Remove(string key)
    {
        if (!Dictionary.Remove(key))
            return false;

        CommandRemoved?.Invoke(key);
        return true;
    }

    public virtual void Add(Action<CommandBuilder> factory)
    {
        var builder = new CommandBuilder();
        factory(builder);
        var command = builder.Build();

        Add(command.Name, command);
    }

    public virtual void Add(KeyValuePair<string, CommandInfo> item)
    {
        Add(item.Key, item.Value);
    }

    public virtual void Add(string key, CommandInfo commandInfo)
    {
        key = key.Trim();
        commandInfo.Description = commandInfo.Description?.Trim();

        if (ContainsKey(key))
        {
            Log.Warning("[commands] Command `{Cmd}` not added (already exist)", key);
            return;
        }

        switch (key.Length)
        {
            case < CommandInfo.MinCommandLength:
                Log.Warning("[commands] Command `{Cmd}` not added: minimum length not reached", key);
                return;
            case > CommandInfo.MaxCommandLength:
                Log.Warning("[commands] Command `{Cmd}` not added: maximum cmd length exceeded", key);
                return;
        }

        if (commandInfo.Description?.Length > CommandInfo.MaxDescriptionLength)
        {
            Log.Warning("[commands] Command `{Cmd}` not added: maximum description length exceeded", key);
            return;
        }

        if (commandInfo.Parameters.Count > CommandInfo.MaxParamsLength)
        {
            Log.Warning("[commands] Command `{Cmd}` not added: maximum parameters length exceeded", key);
            return;
        }

        Dictionary.Add(key, commandInfo);
        CommandAdded?.Invoke(key, commandInfo);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Dictionary).GetEnumerator();
    }
}
