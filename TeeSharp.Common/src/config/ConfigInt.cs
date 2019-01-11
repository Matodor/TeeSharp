namespace TeeSharp.Common.Config
{
    public class ConfigInt : ConfigVariable
    {
        public int Value
        {
            get => _value;
            set => _value = System.Math.Clamp(value, Min, Max);
        }

        public int Min { get; set; }
        public int Max { get; set; }
        public readonly int DefaultValue;

        private int _value;

        public ConfigInt(string cmd, int def, int min, int max,
            ConfigFlags flags, string desc) : base(cmd, flags, desc)
        {
            Min = min;
            Max = max;
            Value = def;
            DefaultValue = def;
        }

        public override string AsString()
        {
            return Value.ToString();
        }

        public override int AsInt()
        {
            return Value;
        }

        public override bool AsBoolean()
        {
            return Value != 0;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}