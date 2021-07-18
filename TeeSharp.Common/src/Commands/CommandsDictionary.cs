using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Serilog;
using TeeSharp.Common.Commands.Parsers;
using TeeSharp.Common.Config;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Common.Commands
{
    /// <summary>
    /// server commands store
    /// command example - /login name pass
    /// key - login
    /// value - Command from "/login name pass"
    /// </summary>
    public class CommandsDictionary : BaseCommandsDictionary
    {
        public override event Action<Command> CommandAdded;

        public Dictionary<string, Command> Dictionary { get; set; } = new Dictionary<string, Command>();
        private readonly ICommandParser _parser = new DefaultCommandParser();

        public override Command this[string command]
        {
            get
            {
                if (Dictionary.ContainsKey(command))
                    return Dictionary[command];
                throw new Exception("Command not found");
            }
            set
            {
                if (Dictionary.ContainsKey(command))
                {
                    Dictionary[command] = value;
                }
                else
                {
                    Dictionary.Add(command, value);
                    CommandAdded?.Invoke(value);
                }
            }
        }

        public override void Add(string cmd, string format, string description, CommandCallback callback)
        {
            if (Dictionary.ContainsKey(cmd))
            {
                Log.Warning($"[commands] Command {cmd} already exist");
            }

            var command = new Command(cmd, format, description);
            command.Executed += callback;
            Dictionary.Add(cmd, command);
            CommandAdded?.Invoke(command);
        }

        public override void SetAccessLevel(int accessLevel, params string[] commands)
        {
            foreach (var t in commands)
            {
                this[t].AccessLevel = accessLevel;
            }
        }

        public override IEnumerable<KeyValuePair<string, Command>> Get(int accessLevel)
        {
            return Dictionary.Where(pair => pair.Value.AccessLevel <= accessLevel);
        }

        public override (bool Ok, Command Command, string Args) Parse(string line)
        {
            var (ok, cmd, args) = _parser.Parse(line);
            if (!ok || !Dictionary.TryGetValue(cmd, out var command))
                return (false, null, null);

            return (true, command, args);
        }

        public override IEnumerator<Command> GetEnumerator()
        {
            return Dictionary.Values.GetEnumerator();
        }
    }
}