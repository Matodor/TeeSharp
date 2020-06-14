using System.Collections;
using System.Collections.Generic;
using TeeSharp.Core;

namespace TeeSharp.Common
{
    public abstract class BaseTuningParams : BaseInterface, IEnumerable<KeyValuePair<string, TuningParameter>>
    {
        public virtual TuningParameter this[string key] => Parameters[key];
        public virtual int Count => Parameters.Count;

        protected virtual IDictionary<string, TuningParameter> Parameters { get; set; }

        public virtual bool Contains(string param) => Parameters.ContainsKey(param);
        public abstract void Reset();
        public abstract IEnumerator<KeyValuePair<string, TuningParameter>> GetEnumerator();

        protected abstract void AppendParameters(IDictionary<string, TuningParameter> parameters);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}