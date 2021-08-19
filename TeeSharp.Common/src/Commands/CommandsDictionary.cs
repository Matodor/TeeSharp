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
    /// Commands store
    /// Command example - /login name pass
    /// key - login
    /// value - Command from "/login name pass"
    /// </summary>
    public class CommandsDictionary : BaseCommandsDictionary
    {
        public override CommandInfo this[string key]
        {
            get => Dictionary[key];
            set
            {
                if (ContainsKey(key))
                {
                    Dictionary[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }
        
        public override event Action<string, CommandInfo> CommandAdded;
        public override event Action<string> CommandRemoved;
        
        public override ICollection<string> Keys => Dictionary.Keys;

        public override ICollection<CommandInfo> Values => Dictionary.Values;

        public override int Count => Dictionary.Count;

        public override bool IsReadOnly => Dictionary.IsReadOnly;
        
        protected IDictionary<string, CommandInfo> Dictionary { get; set; } 
            = new Dictionary<string, CommandInfo>();

        public override void Init()
        {
            
        }

        public override void Clear()
        {
            Dictionary.Clear();
        }

        public override IEnumerator<KeyValuePair<string, CommandInfo>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public override bool TryGetValue(string key, out CommandInfo value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public override bool Contains(KeyValuePair<string, CommandInfo> item)
        {
            return Dictionary.Contains(item);
        }

        public override bool ContainsKey(string key)
        {
            return Dictionary.ContainsKey(key);
        }

        public override void CopyTo(KeyValuePair<string, CommandInfo>[] array, int arrayIndex)
        {
            Dictionary.CopyTo(array, arrayIndex);
        }

        public override bool Remove(KeyValuePair<string, CommandInfo> item)
        {
            return Remove(item.Key);
        }
        
        public override bool Remove(string key)
        {
            if (!Dictionary.Remove(key)) 
                return false;
            
            CommandRemoved?.Invoke(key);
            return true;
        }
        
        public override void Add(string cmd, string parametersPattern, 
            CommandHandler callback, string description = "")
        {
            var command = new CommandInfo(parametersPattern, description);
            command.Executed += callback;
            
            Add(cmd, command);
        }

        public override void Add(KeyValuePair<string, CommandInfo> item)
        {
            Add(item.Key, item.Value);
        }

        public override void Add(string key, CommandInfo commandInfo)
        {
            key = key.Trim();
            commandInfo.Description = commandInfo.Description.Trim();
            
            if (ContainsKey(key))
            {
                Log.Warning("[commands] Command `{Cmd}` not added (already exist)", key);
                return;
            }

            if (key.Length < CommandInfo.MinCommandLength)
            {
                Log.Warning("[commands] Command `{Cmd}` not added: minimum length not reached", key);
                return;
            }

            if (key.Length > CommandInfo.MaxCommandLength)
            {
                Log.Warning("[commands] Command `{Cmd}` not added: maximum cmd length exceeded", key);
                return;
            }

            if (commandInfo.Description.Length > CommandInfo.MaxDescriptionLength)
            {
                Log.Warning("[commands] Command `{Cmd}` not added: maximum description length exceeded", key);
                return;
            }

            if (commandInfo.ParametersPattern.Length > CommandInfo.MaxParamsLength)
            {
                Log.Warning("[commands] Command `{Cmd}` not added: maximum parameters length exceeded", key);
                return;
            }
            
            Dictionary.Add(key, commandInfo);
            CommandAdded?.Invoke(key, commandInfo);
        }
    }
}