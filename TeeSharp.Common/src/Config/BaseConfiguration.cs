using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Common.Config;

public abstract class BaseConfiguration : IContainerService, 
    IReadOnlyDictionary<string, ConfigVariable>
{
    public Container.IScope Container { get; set; }
    public abstract int Count { get; }
        
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public abstract void Init();
    public abstract void LoadConfig(FileStream fs);
    public abstract bool TrySetValue(JProperty property);
    public abstract bool TrySetValue(string variableName, ConfigVariable value);
    public abstract bool TrySetValue(string variableName, bool value);
    public abstract bool TrySetValue(string variableName, string value);
    public abstract bool TrySetValue(string variableName, float value);
    public abstract bool TrySetValue(string variableName, int value);
    public abstract T GetOrCreate<T>(string variableName) where T : ConfigVariable, new();
    public abstract T GetOrAdd<T>(string variableName, T variable) where T : ConfigVariable, new();
        
    public abstract IEnumerator<KeyValuePair<string, ConfigVariable>> GetEnumerator();
    public abstract bool ContainsKey(string variableName);
    public abstract bool TryGetValue(string variableName, out ConfigVariable value);
    public abstract bool TryGetValue<T>(string variableName, out T value) where T : ConfigVariable;
    public abstract ConfigVariable this[string key] { get; }
    public abstract IEnumerable<string> Keys { get; }
    public abstract IEnumerable<ConfigVariable> Values { get; }
}