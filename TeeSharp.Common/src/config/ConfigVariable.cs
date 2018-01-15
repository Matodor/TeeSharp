namespace TeeSharp.Common.Config
{
    public abstract class ConfigVariable
    {
        public readonly string Name;
        public readonly string ConsoleCommand;
        public readonly ConfigFlags Flags;
        public readonly string Description;

        public abstract string AsString();
        public abstract int AsInt();
        public abstract bool AsBoolean();

        protected ConfigVariable(string name, string cmd, ConfigFlags flags, string desc)
        {
            Name = name;
            ConsoleCommand = cmd;
            Flags = flags;
            Description = desc;
        }
    }
}