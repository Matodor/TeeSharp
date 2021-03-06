using System;

namespace TeeSharp.Common.Config
{
    public class ConfigVariableFloat : ConfigVariable<float>
    {
        public override event Action<float> OnChange;

        public override float DefaultValue { get; }

        public float Min
        {
            get => _min;
            set
            {
                _min = value;
                
                if (_min != _max)
                    _value = Math.Clamp(_value, _min, _max);
            }
        }

        public float Max
        {
            get => _max;
            set
            {
                _max = value;
                
                if (_min != _max)
                    _value = Math.Clamp(_value, _min, _max);
            }
        }
        
        public override float Value
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

        private float _defaultValue = 0;
        private float _value;
        private float _min;
        private float _max;

        public ConfigVariableFloat(float defaultValue, string description = "", float min = 0, float max = 0)
        {
            DefaultValue = defaultValue;
            Description = description;

            _min = min;
            _max = max;
            _value = _min != _max
                ? Math.Clamp(_defaultValue, _min, _max)
                : _defaultValue;
        }
        
        public override object GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}