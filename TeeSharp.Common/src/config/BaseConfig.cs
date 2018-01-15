using System.Collections.Generic;

namespace TeeSharp.Common.Config
{
    public abstract class BaseConfig : BaseInterface
    {
        public virtual ConfigVariable this[string key] => _variables[key];
        public virtual Dictionary<string, ConfigVariable>.KeyCollection Keys => _variables.Keys;
        public virtual IReadOnlyDictionary<string, ConfigVariable> Variables => _variables;

        private readonly Dictionary<string, ConfigVariable> _variables;

        protected BaseConfig()
        {
            _variables = new Dictionary<string, ConfigVariable>();
        }

        protected virtual void AppendVariables(IDictionary<string, ConfigVariable> variables)
        {
            foreach (var pair in variables)
            {
                if (!_variables.TryAdd(pair.Key, pair.Value))
                    Debug.Log("config", $"Variable '{pair.Key}' already ");
            }
        }

        protected virtual void Reset()
        {
            foreach (var pair in _variables)
            {
                if (pair.Value is ConfigString strCfg)
                    strCfg.Value = strCfg.DefaultValue;
                else if (pair.Value is ConfigInt intCfg)
                    intCfg.Value = intCfg.DefaultValue;
            }
        }
    }
}