using System.Collections;
using System.Collections.Generic;
using TeeSharp.Core;

namespace TeeSharp.Common.Config
{
    public abstract class BaseConfig : BaseInterface, IEnumerable<KeyValuePair<string, ConfigVariable>>
    {
        public virtual ConfigVariable this[string key] => Variables[key];

        protected virtual IDictionary<string, ConfigVariable> Variables { get; set; }
        protected virtual ConfigFlags SaveMask { get; set; }

        protected abstract void AppendVariables(IDictionary<string, ConfigVariable> variables);
        protected abstract void Reset();

        public abstract void Init(ConfigFlags saveMask);
        public abstract void Save(string fileName);
        public abstract void RestoreString();
        public abstract IEnumerator<KeyValuePair<string, ConfigVariable>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}