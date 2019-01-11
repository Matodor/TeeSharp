namespace TeeSharp.Common.Config
{
    public class ConfigString : ConfigVariable
    {
        public int MaxLength { get; set; }
        public string Value { get; set; }
        public readonly string DefaultValue;

        public ConfigString(string cmd, int maxLength, string def,
            ConfigFlags flags, string desc) : base(cmd, flags, desc)
        {
            MaxLength = maxLength;
            Value = def;
            DefaultValue = def;
        }

        public override string AsString()
        {
            return Value;
        }

        public override int AsInt()
        {
            throw new System.NotImplementedException();
        }

        public override bool AsBoolean()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return Value;
        }
    }
}