using TeeSharp.Common.Config;

namespace TeeSharp.Common.Console
{
    public class ConsoleCommand
    {
        public const string ARGUMENTS_TYPES = "sfi?"; // s - string, f - float, i - integer

        public readonly string Cmd;
        public readonly string Format;
        public readonly ConfigFlags Flags;
        public readonly string Description;
        public readonly ConsoleCallback Callback;
        public readonly object Data;

        public ConsoleCommand(string cmd, string format, ConfigFlags flags, 
            string description, ConsoleCallback callback, object data)
        {
            Cmd = cmd;
            Format = format;
            Flags = flags;
            Description = description;
            Callback = callback;
            Data = data;
        }
    }
}