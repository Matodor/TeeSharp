using System;

namespace TeeSharp.Common.Config;

public class ConfigVariableBoolean : ConfigVariable<bool>
{
    public override event Action<bool> OnChange;

    public override bool DefaultValue { get; }

    public override bool Value
    {
        get => _value;
        set
        {
            _value = value;
            OnChange?.Invoke(_value);
        }
    }

    private bool _value;

    public ConfigVariableBoolean(bool defaultValue, string description = "")
    {
        DefaultValue = defaultValue;
        Description = description;

        _value = defaultValue;
    }

    public override object GetValue()
    {
        return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
        
    public static implicit operator bool(ConfigVariableBoolean that) => that._value;
}