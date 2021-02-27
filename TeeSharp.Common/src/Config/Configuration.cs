using System.Collections.Generic;

namespace TeeSharp.Common.Config
{
    public class Configuration : BaseConfiguration
    {
        public override ConfigurationItem this[string key]
        {
            get
            {
                return null;
            }
        }

        public override IEnumerable<string> Keys => Items.Keys;
        public override IEnumerable<ConfigurationItem> Values => Items.Values;
        public override int Count => Items.Count;

        protected IDictionary<string, ConfigurationItem> Items { get; set; }
        
        public override IEnumerator<KeyValuePair<string, ConfigurationItem>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public override void Init()
        {
            Items = new Dictionary<string, ConfigurationItem>();
        }

        public override bool ContainsKey(string key)
        {
            return Items.ContainsKey(key);
        }

        public override bool TryGetValue(string key, out ConfigurationItem value)
        {
            return Items.TryGetValue(key, out value);
        }
    }
}