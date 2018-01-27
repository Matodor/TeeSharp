using System.Collections;
using System.Collections.Generic;
using TeeSharp.Core;

namespace TeeSharp.Common.Config
{
    public abstract class BaseConfig : BaseInterface, IEnumerable<KeyValuePair<string, ConfigVariable>>
    {
        public virtual ConfigVariable this[string key] => Variables[key];

        protected virtual IDictionary<string, ConfigVariable> Variables { get; set; }

        protected BaseConfig()
        {
            Variables = new Dictionary<string, ConfigVariable>();
        }

        protected virtual void AppendVariables(IDictionary<string, ConfigVariable> variables)
        {
            foreach (var pair in variables)
            {
                if (!Variables.TryAdd(pair.Key, pair.Value))
                    Debug.Log("config", $"Variable '{pair.Key}' already added");
            }
        }

        protected virtual void Reset()
        {
            foreach (var pair in Variables)
            {
                if (pair.Value is ConfigString strCfg)
                    strCfg.Value = strCfg.DefaultValue;
                else if (pair.Value is ConfigInt intCfg)
                    intCfg.Value = intCfg.DefaultValue;
            }
        }

        public IEnumerator<KeyValuePair<string, ConfigVariable>> GetEnumerator()
        {
            return Variables.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}