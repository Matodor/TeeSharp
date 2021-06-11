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
            var variableType = typeof(ConfigVariable);
            var bindingFlags = 
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.GetProperty |
                BindingFlags.GetField;
            
            var thatType = typeof(Configuration);
            var baseType = typeof(BaseConfiguration);
            
            // ReSharper disable once ArrangeThisQualifier
            var fields = this
                .GetType()
                .GetFields(bindingFlags)
                .Where(f => 
                    f.DeclaringType != thatType && 
                    f.DeclaringType != baseType &&
                    variableType.IsAssignableFrom(f.FieldType)
                )
                .ToArray();
            
            // ReSharper disable once ArrangeThisQualifier
            var properties = this
                .GetType()
                .GetProperties(bindingFlags)
                .Where(p => 
                    p.DeclaringType != thatType && 
                    p.DeclaringType != baseType &&
                    variableType.IsAssignableFrom(p.PropertyType)
                )
                .ToArray();

            foreach (var fieldInfo in fields)
                Variables.Add(fieldInfo.Name, (ConfigVariable) fieldInfo.GetValue(this)); 
            
            foreach (var propertyInfo in properties)
                Variables.Add(propertyInfo.Name, (ConfigVariable) propertyInfo.GetValue(this)); 
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

            Log.Debug("[config] Loading config from file stream");

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
                    Log.Debug("[config] Ignore variable at path `{Path}`", token.Path);
                    continue;
                }

                if (!ContainsKey(property.Name))
                {
                    Log.Debug("[config] Skip unknown variable `{Name}`", property.Name);
                    continue;
                }

                if (!TrySetValue(property))
                {
                    Log.Debug("[config] Setting variable {Name} failed ({Type} -> {TypeName})",
                        property.Name,
                        property.Value.Type,
                        this[property.Name].GetType().Name
                    );
                    continue;
                }

                Log.Debug("[config] Set {Name} - {Value}", property.Name, this[property.Name].GetValue());
            }

            Log.Information("[config] Config loaded succesfully");
        }

        public override bool TrySetValue(JProperty property)
        {
            if (!ContainsKey(property.Name))
                return false;

            switch (property.Value.Type)
            {
                case JTokenType.Boolean:
                    return TrySetValue(property.Name, property.Value.Value<bool>());
                    
                case JTokenType.String:
                    return TrySetValue(property.Name, property.Value.Value<string>());

                case JTokenType.Float:
                    return TrySetValue(property.Name, property.Value.Value<float>());

                case JTokenType.Integer:
                    return Variables[property.Name] switch
                    {
                        ConfigVariableFloat _ => TrySetValue(property.Name, (float) property.Value.Value<int>()),
                        ConfigVariableInteger _ => TrySetValue(property.Name, property.Value.Value<int>()),
                        _ => false
                    };

                default:
                    Log.Warning("[config] Ignore variable with type `{Type}`, path `{Path}`", property.Value.Type, property.Path);
                    return false;
            }
        }

        public override bool TrySetValue(string variableName, ConfigVariable value)
        {
            if (!ContainsKey(variableName))
                return false;

            Variables[variableName] = value;
            return true;
        }

        public override bool TrySetValue(string variableName, bool value)
        {
            if (!TryGetValue<ConfigVariableBoolean>(variableName, out var variable)) 
                return false;
            
            variable.Value = value;
            return true;
        }

        public override bool TrySetValue(string variableName, string value)
        {
            if (!TryGetValue<ConfigVariableString>(variableName, out var variable)) 
                return false;
            
            variable.Value = value;
            return true;
        }

        public override bool TrySetValue(string variableName, float value)
        {
            if (!TryGetValue<ConfigVariableFloat>(variableName, out var variable)) 
                return false;
            
            variable.Value = value;
            return true;
        }

        public override bool TrySetValue(string variableName, int value)
        {
            if (!TryGetValue<ConfigVariableInteger>(variableName, out var variable)) 
                return false;
            
            variable.Value = value;
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
            if (TryGetValue(variableName, out var result) && result is T variable)
            {
                value = variable;
                return true;
            }

            value = null;
            return false;
        }
    }
}