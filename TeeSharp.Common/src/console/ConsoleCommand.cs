﻿using System;
using TeeSharp.Common.Config;

namespace TeeSharp.Common.Console
{
    public delegate void CommandCallback(ConsoleCommandResult commandResult, int clientId, ref object data);

    public class ConsoleCommand
    {
        public event CommandCallback Executed;

        public const int MaxCmdLength = 32;
        public const int MaxDescLength = 96;
        public const int MaxParamsLength = 16;

        public const string ArgumentsTypes = "sfi?r"; // s - string, f - float, i - integer, ? - optional, r - rest of the string

        public int AccessLevel { get; set; }
        public readonly string Cmd;

        public readonly string Format;
        public readonly ConfigFlags Flags;
        public readonly string Description;

        public object Data;

        public ConsoleCommand(
            string cmd, 
            string format, 
            string description, 
            ConfigFlags flags,
            object data)
        {
            Cmd = cmd.Trim();
            Format = format.Trim().Replace("??", "?");
            Flags = flags;
            Description = description;
            Data = data;

            if (string.IsNullOrEmpty(Cmd))
                throw new Exception("ConsoleCommand empty cmd");
        }

        public void Invoke(ConsoleCommandResult result, int clientId)
        {
            Executed?.Invoke(result, clientId, ref Data);
        }
    }
}