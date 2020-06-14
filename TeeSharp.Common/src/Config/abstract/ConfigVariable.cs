namespace TeeSharp.Common.Config
{
    public abstract class ConfigVariable
    {
        public readonly string ConsoleCommand;
        public readonly ConfigFlags Flags;
        public readonly string Description;

        public abstract string AsString();
        public abstract int AsInt();
        public abstract bool AsBoolean();

        public static implicit operator string(ConfigVariable v)
        {
            return v.AsString();
        }

        public static implicit operator int(ConfigVariable v)
        {
            return v.AsInt();
        }

        public static implicit operator bool(ConfigVariable v)
        {
            return v.AsBoolean();
        }

        protected ConfigVariable(string cmd, ConfigFlags flags, string desc)
        {
            ConsoleCommand = cmd;
            Flags = flags;
            Description = desc;
        }
    }
}