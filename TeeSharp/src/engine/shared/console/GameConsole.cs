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

        public void Init()
        {
            _storage = Kernel.RequestSingleton<IStorage>();
            _config = Kernel.RequestSingleton<Configuration>();

            foreach (var pair in _config.Variables)
            {
                var configInt = pair.Value as ConfigInt;
                if (configInt != null)
                {
                    RegisterCommand(configInt.ConsoleCommand, "int?", configInt.Flags,
                        IntVariableCommandCallback, configInt, configInt.Description, AccessLevel.ADMIN);
                    continue;
                }

                var configStr = pair.Value as ConfigStr;
                if (configStr != null)
                {
                    RegisterCommand(configStr.ConsoleCommand, "string?", configStr.Flags,
                        IntVariableCommandCallback, configStr, configStr.Description, AccessLevel.ADMIN);
                }
            }

        }

        private void StrVariableCommandCallback(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        private void IntVariableCommandCallback(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        public void ExecuteFile(string path)
        {
            throw new NotImplementedException();
        }

        public void ParseArguments(string[] args)
        {
            throw new NotImplementedException();
        }

        public void RegisterCommand(string command, string formatArguments, Configuration.ConfigFlags flags, 
            ConsoleCallback callback, object data, string description, AccessLevel accessLevel)
        {
            if (_commands.ContainsKey(command))
                return;

            _commands.Add(command, new ConsoleCommand(
                command,
                formatArguments,
                flags,
                callback,
                description,
                accessLevel
            ));
        }
    }
}
