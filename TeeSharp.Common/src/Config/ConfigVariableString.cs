using System;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Common.Config
{
    public class ConfigVariableString : ConfigVariable<string>
    {
        public override event Action<string> OnChange;

        public int MaxLength
        {
            get => _maxLength;
            set
            {
                Value = Value.Limit(value);
                _maxLength = value;
            }
        }

        public override string DefaultValue { get; }

        public override string Value
        {
            get => _value;
            set
            {
                _value = value.Limit(MaxLength);
                OnChange?.Invoke(_value);
            }
        }

        private string _value;
        private int _maxLength;

        public ConfigVariableString(string defaultValue, string description = "", int maxLength = -1)
        {
            DefaultValue = defaultValue;
            Description = description;

            _maxLength = maxLength;
            _value = defaultValue.Limit(maxLength);
        }
        
        public override object GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}