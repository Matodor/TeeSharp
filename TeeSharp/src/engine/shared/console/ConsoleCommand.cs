using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public class ConsoleCommand
    {
        public readonly string Command;
        public readonly string FormatArguments;
        public readonly Configuration.ConfigFlags Flags;
        public readonly ConsoleCallback Callback;
        public readonly string Description;
        public readonly AccessLevel AccessLevel;

        public ConsoleCommand(string command, string formatArguments, Configuration.ConfigFlags flags,
            ConsoleCallback callback, string description, AccessLevel accessLevel)
        {
            Command = command;
            FormatArguments = formatArguments;
            Flags = flags;
            Callback = callback;
            Description = description;
            AccessLevel = accessLevel;
        }
    }
}
