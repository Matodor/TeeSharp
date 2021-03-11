using System;

namespace TeeSharp.Common.Config
{
    public class ConfigVariableInteger : ConfigVariable<int>
    {
        public override event Action<int> OnChange;

        public override int DefaultValue { get; }

        public int Min
        {
            get => _min;
            set
            {
                _min = value;
                
                if (_min != _max)
                    _value = Math.Clamp(_value, _min, _max);
            }
        }

        public int Max
        {
            get => _max;
            set
            {
                _max = value;
                
                if (_min != _max)
                    _value = Math.Clamp(_value, _min, _max);
            }
        }
        
        public override int Value
        {
            get => _value;
            set
            {
                _value = _min != _max
                    ? Math.Clamp(value, _min, _max)
                    : value;
                
                OnChange?.Invoke(_value);
            }
        }

        private int _value;
        private int _min;
        private int _max;

        public ConfigVariableInteger(int defaultValue, string description = "", int min = 0, int max = 0)
        {
            DefaultValue = defaultValue;
            Description = description;

            _min = min;
            _max = max;
            _value = _min != _max
                ? Math.Clamp(defaultValue, _min, _max)
                : defaultValue;
        }
        
        public override object GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator int(ConfigVariableInteger that) => that._value;
    }
}