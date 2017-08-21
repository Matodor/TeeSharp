using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public class GameConsole : IGameConsole
    {
        protected IStorage _storage;
        protected Configuration _config;

        protected readonly IDictionary<string, ConsoleCommand> _commands;

        public GameConsole()
        {
            _commands = new Dictionary<string, ConsoleCommand>();
        }

        public virtual void Init()
        {
            _storage = Kernel.Get<IStorage>();
            _config = Kernel.Get<Configuration>();

            foreach (var pair in _config.Variables)
            {
                var configInt = pair.Value as ConfigInt;
                if (configInt != null)
                {
                    RegisterCommand(configInt.ConsoleCommand, "int?", configInt.Flags,
                        IntVariableCommandCallback, configInt, configInt.Description, ConsoleAccessLevel.ADMIN);
                    continue;
                }

                var configStr = pair.Value as ConfigStr;
                if (configStr != null)
                {
                    RegisterCommand(configStr.ConsoleCommand, "string?", configStr.Flags,
                        StrVariableCommandCallback, configStr, configStr.Description, ConsoleAccessLevel.ADMIN);
                }
            }

        }

        public virtual ConsoleCommand GetCommand(string command)
        {
            return _commands.ContainsKey(command) ? _commands[command] : null;
        }

        public virtual void OnExecuteCommand(string command, ConsoleCallback callback)
        {
            var consoleCommand = GetCommand(command);
            if (consoleCommand == null)
            {
                Print(ConsoleOutputLevel.DEBUG, "console", $"failed to chain '{command}'");
                return;
            }
            consoleCommand.Callback += callback;
        }

        public virtual void Print(ConsoleOutputLevel level, string from, string str)
        {
            
        }
        
        protected virtual void StrVariableCommandCallback(ConsoleResult result, object data)
        {
            var configStr = (ConfigStr) data;
            if (result.NumArguments != 0)
                configStr.Default = result.GetString(0);
            else
                Print(ConsoleOutputLevel.STANDARD, "console", $"Value: {configStr.Default}");
        }

        protected virtual void IntVariableCommandCallback(ConsoleResult result, object data)
        {
            var configInt = (ConfigInt) data;
            if (result.NumArguments != 0)
                configInt.Default = Math.Clamp(result.GetInteger(0), configInt.Min, configInt.Max);
            else
                Print(ConsoleOutputLevel.STANDARD, "console", $"Value: {configInt.Default}");
        }

        public virtual void ExecuteFile(string path)
        {
            throw new NotImplementedException();
        }

        public virtual void ParseArguments(string[] args)
        {
            throw new NotImplementedException();
        }

        public virtual void RegisterCommand(string command, string formatArguments, ConfigFlags flags, 
            ConsoleCallback callback, object data, string description, ConsoleAccessLevel accessLevel)
        {
            if (_commands.ContainsKey(command))
                return;

            _commands.Add(command, new ConsoleCommand(
                command,
                formatArguments,
                flags,
                callback,
                data,
                description,
                accessLevel
            ));
        }
    }
}
