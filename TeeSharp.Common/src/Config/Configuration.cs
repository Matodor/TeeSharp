using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace TeeSharp.Common.Config
{
    public class Configuration : BaseConfiguration
    {
        public override ConfigVariable this[string key] =>
            TryGetValue(key, out var variable)
                ? variable
                : null;

        public override IEnumerable<string> Keys => Variables.Keys;
        public override IEnumerable<ConfigVariable> Values => Variables.Values;
        public override int Count => Variables.Count;

        protected IDictionary<string, ConfigVariable> Variables { get; set; }

        protected Configuration()
        {
            Variables = new Dictionary<string, ConfigVariable>();
        }

        public override void Init()
        {
            // ReSharper disable once ArrangeThisQualifier
            var properties = this
                .GetType()
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.GetProperty
                )
                .Where(p =>
                    p.PropertyType == typeof(ConfigVariableString) ||
                    p.PropertyType == typeof(ConfigVariableFloat) ||
                    p.PropertyType == typeof(ConfigVariableInteger)
                )
                .ToArray();

            foreach (var propertyInfo in properties)
            {
                if (!ContainsKey(propertyInfo.Name))
                    Variables.Add(propertyInfo.Name, (ConfigVariable) propertyInfo.GetValue(this));
            }
        }

        public override void LoadConfig(FileStream fs)
        {
            if (fs.CanSeek)
                fs.Seek(0, SeekOrigin.Begin);

            if (!fs.CanRead)
            {
                Log.Debug("[config] Can read given stream");
                return;
            }

            Log.Debug("[config] Start loading config from file stream");
            
            using var streamReader = new StreamReader(fs);
            using var jsonReader = new JsonTextReader(streamReader);
            JObject config;

            try
            {
                config = JObject.Load(jsonReader);
            }
            catch (Exception e)
            {
                Log.Debug(e, "[config] Loading config error");
                return;
            }

            foreach (var token in config.Children())
            {
                if (!(token is JProperty property))
                {
                    Log.Debug($"[config] Ignore variable at path `{token.Path}`");
                    continue;
                }

                if (!TrySetValue(property))
                    continue;
                
                Log.Debug($"[config] Set `{property.Name}` = {this[property.Name].GetValue()}");
            }
            
            Log.Debug("[config] Config loaded succesfully");
        }

        public override bool TrySetValue(JProperty property)
        {
            switch (property.Value.Type)
            {
                case JTokenType.String:
                    return TrySetValue(property.Name, property.Value.Value<string>());
                case JTokenType.Float:
                    return TrySetValue(property.Name, property.Value.Value<float>());
                case JTokenType.Integer:
                    return TrySetValue(property.Name, property.Value.Value<int>());
                default:
                    Log.Warning($"[config] Ignore variable with type `{property.Value.Type}`, path `{property.Path}`");
                    return false;
            }
        }

        public override bool TrySetValue(string variableName, string value)
        {
            GetOrCreate<ConfigVariableString>(variableName).Value = value;
            return true;
        }

        public override bool TrySetValue(string variableName, float value)
        {
            return true;
        }

        public override bool TrySetValue(string variableName, int value)
        {
            return true;
        }

        public override T GetOrCreate<T>(string variableName)
        {
            if (TryGetValue<T>(variableName, out var result))
                return result;

            Variables.Add(variableName, new T());
            return (T) Variables[variableName];
        }

        public override T GetOrAdd<T>(string variableName, T variable)
        {
            if (TryGetValue<T>(variableName, out var result))
                return result;
            
            Variables.Add(variableName, variable);
            return variable;
        }

        public override IEnumerator<KeyValuePair<string, ConfigVariable>> GetEnumerator()
        {
            return Variables.GetEnumerator();
        }

        public override bool ContainsKey(string variableName)
        {
            return Variables.ContainsKey(variableName);
        }

        public override bool TryGetValue(string variableName, out ConfigVariable value)
        {
            return Variables.TryGetValue(variableName, out value);
        }

        public override bool TryGetValue<T>(string variableName, out T value)
        {
            if (TryGetValue(variableName, out var result))
            {
                // ReSharper disable once InvertIf
                if (result is T variable)
                {
                    value = variable;
                    return true;
                }

                throw new Exception($"The expected type of the variable is `{typeof(T).FullName}`");
            }

            value = null;
            return false;
        }
    }
}