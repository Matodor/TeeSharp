using System;

namespace TeeSharp.Common.Config
{
    public abstract class ConfigVariable
    {
        public ConfigVariableFlags Flags { get; set; } = ConfigVariableFlags.None;
        public string Description { get; set; } = string.Empty;
        
        public abstract object GetValue();
    }
}