using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public class ConsoleCommand
    {
        public event ConsoleCallback Callback;

        public readonly string Command;
        public readonly string FormatArguments;
        public readonly ConfigFlags Flags;
        public readonly string Description;
        public readonly ConsoleAccessLevel AccessLevel;
        public readonly object Data;

        public ConsoleCommand(string command, string formatArguments, ConfigFlags flags,
            ConsoleCallback callback, object data, string description, ConsoleAccessLevel accessLevel)
        {
            Command = command;
            FormatArguments = formatArguments;
            Flags = flags;
            Callback = callback;
            Description = description;
            AccessLevel = accessLevel;
            Data = data;
        }
    }
}
