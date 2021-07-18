using System;
using System.Collections;
using System.Collections.Generic;
using TeeSharp.Common.Config;

namespace TeeSharp.Common.Commands
{
    public abstract class BaseCommandsDictionary : IEnumerable<Command>
    {
        public abstract event Action<Command> CommandAdded;
        
        public abstract Command this[string command] { get; set; }

        public abstract void Add(string cmd, string format, string description, CommandCallback callback);
        public abstract void SetAccessLevel(int accessLevel, params string[] commands);
        public abstract IEnumerable<KeyValuePair<string, Command>> Get(int accessLevel);
        public abstract (bool Ok, Command Command, string Args) Parse(string line);
        public abstract IEnumerator<Command> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}