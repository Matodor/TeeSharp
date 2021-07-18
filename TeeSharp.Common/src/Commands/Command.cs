using System;
using System.Collections.Generic;
using TeeSharp.Common.Config;

namespace TeeSharp.Common.Commands
{
    public delegate void CommandCallback(IEnumerable<object> commandResult, Command command);
    public class Command
    {
        public event CommandCallback Executed;
        
        public const int MaxCmdLength = 32;
        public const int MaxDescLength = 96;
        public const int MaxParamsLength = 16;
        
        public int AccessLevel { get; set; }
        public string Cmd { get; set; }
        public string Pattern{ get; set; }
        public string Description { get; set; }
        
        public Command(
            string cmd, 
            string format, 
            string description)
        {
            Cmd = cmd.Trim();
            Pattern = format.Trim().Replace("??", "?");
            Description = description;

            if (string.IsNullOrEmpty(Cmd))
                throw new Exception("ConsoleCommand empty cmd");
        }

        public void Invoke(IEnumerable<object>  arguments)
        {
            Executed?.Invoke(arguments, this);
        }
    }
}