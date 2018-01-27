namespace TeeSharp.Common
{
    public class TuningParameter
    {
        public readonly string Key;
        public readonly int DefaultValue;

        public int Value { get; set; }

        public float FloatValue
        {
            get => Value / 100.00f;
            set => Value = (int) System.Math.Round(value * 100f);
        }

        public TuningParameter(string key, string scriptName, float defaultValue)
        {
            Key = key;
            DefaultValue = (int) System.Math.Round(defaultValue * 100f);
            Value = DefaultValue;
        }

        public static implicit operator float(TuningParameter v)
        {
            return v.FloatValue;
        }
    }
}