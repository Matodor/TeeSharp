using System.Collections;
using System.Collections.Generic;
using TeeSharp.Core;

namespace TeeSharp.Common
{
    public class BaseTuningParams : BaseInterface, IEnumerable<KeyValuePair<string, TuningParameter>>
    {
        public virtual TuningParameter this[string key] => Parameters[key];
        public virtual int Count => Parameters.Count;

        protected virtual IDictionary<string, TuningParameter> Parameters { get; set; }

        protected BaseTuningParams()
        {
            Parameters = new Dictionary<string, TuningParameter>();
        }

        protected virtual void AppendParameters(
            IDictionary<string, TuningParameter> parameters)
        {
            foreach (var pair in parameters)
            {
                if (!Parameters.TryAdd(pair.Key, pair.Value))
                    Debug.Log("tuning", $"Parameter '{pair.Key}' already added");
            }
        }

        public virtual void Reset()
        {
            foreach (var pair in Parameters)
            {
                pair.Value.Value = pair.Value.DefaultValue;
            }
        }

        public virtual IEnumerator<KeyValuePair<string, TuningParameter>> GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}