using TeeSharp.Core.Extensions;

namespace TeeSharp.Common.Config
{
    public class ConfigVariableString : ConfigVariable
    {
        public int MaxLength { get; set; } = -1;

        public string DefaultValue
        {
            get => _defaultValue;
            set
            {
                if (string.IsNullOrEmpty(Value))
                    Value = value;

                _defaultValue = value;
            }
        }

        public string Value
        {
            get => _value;
            set => _value = value.Limit(MaxLength);
        }

        private string _defaultValue = string.Empty;
        private string _value = string.Empty;
        
        public override object GetValue()
        {
            return Value;
        }
    }
}