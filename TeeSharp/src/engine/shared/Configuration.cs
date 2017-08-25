using System;
using System.Collections.Generic;
using System.Linq;

namespace TeeSharp
{
    public abstract class ConfigVariable
    {
        public string Name;
        public string ConsoleCommand;
        public ConfigFlags Flags;
        public string Description;
    }

    public class ConfigInt : ConfigVariable
    {
        public int Default;
        public int Min;
        public int Max;

        public ConfigInt(string name, string consoleCommand, int def, int min, int max, 
            ConfigFlags flags, string desc)
        {
            Name = name;
            ConsoleCommand = consoleCommand;
            Default = def;
            Min = min;
            Max = max;
            Flags = flags;
            Description = desc;
        }
    }

    public class ConfigStr : ConfigVariable
    {
        public int MaxLength;
        public string Default;

        public ConfigStr(string name, string consoleCommand, int maxLength, string def, 
            ConfigFlags flags, string desc)
        {
            Name = name;
            ConsoleCommand = consoleCommand;
            MaxLength = maxLength;
            Default = def;
            Flags = flags;
            Description = desc;
        }
    }

    [Flags]
    public enum ConfigFlags
    {
        SAVE = 1 << 0,
        CLIENT = 1 << 1,
        SERVER = 1 << 2,
        STORE = 1 << 3,
        MASTER = 1 << 4,
        ECON = 1 << 5,
    }

    public partial class Configuration
    {
        public IReadOnlyList<KeyValuePair<string, object>> Variables => _variablesDictionary.ToList().AsReadOnly();

        private readonly Dictionary<string, object> _variablesDictionary = new Dictionary<string, object>();

        public Configuration()
        {
            foreach (var o in _default_variablesDictionary)
                _variablesDictionary.Add(o.Key, o.Value);
        }

        public virtual int GetInt(string name)
        {
            if (_variablesDictionary.ContainsKey(name))
                return ((ConfigInt)_variablesDictionary[name]).Default;
            return 0;
        }

        public virtual string GetString(string name)
        {
            if (_variablesDictionary.ContainsKey(name))
                return ((ConfigStr)_variablesDictionary[name]).Default;
            return "";
        }

        public virtual void SetInt(string name, int value)
        {
            if (_variablesDictionary.ContainsKey(name))
            {
                var c = (ConfigInt)_variablesDictionary[name];
                c.Default = Math.Clamp(value, c.Min, c.Max);
            }
        }

        public virtual void SetString(string name, string value)
        {
            if (_variablesDictionary.ContainsKey(name))
                ((ConfigStr)_variablesDictionary[name]).Default = value;
        }
    }
}
