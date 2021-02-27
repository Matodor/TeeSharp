using System.Collections;
using System.Collections.Generic;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Common.Config
{
    public abstract class BaseConfiguration : IContainerService, 
        IReadOnlyDictionary<string, ConfigurationItem>
    {
        public Container Container { get; set; }
        public abstract int Count { get; }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract void Init();
        public abstract IEnumerator<KeyValuePair<string, ConfigurationItem>> GetEnumerator();
        public abstract bool ContainsKey(string key);
        public abstract bool TryGetValue(string key, out ConfigurationItem value);
        public abstract ConfigurationItem this[string key] { get; }
        public abstract IEnumerable<string> Keys { get; }
        public abstract IEnumerable<ConfigurationItem> Values { get; }
    }
}