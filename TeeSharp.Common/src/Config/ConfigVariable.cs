using System;

namespace TeeSharp.Common.Config
{
    public abstract class ConfigVariable
    {
        public ConfigVariableFlags Flags { get; set; } = ConfigVariableFlags.None;
        public string Description { get; set; } = string.Empty;
        
        public abstract object GetValue();
    }
    
    public abstract class ConfigVariable<T> : ConfigVariable
    {
        public abstract event Action<T> OnChange;
        
        public abstract T DefaultValue { get; }
        public abstract T Value { get; set; }
    }
}