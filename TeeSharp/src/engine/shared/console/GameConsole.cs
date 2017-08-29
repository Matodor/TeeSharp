using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public class PrintCallback
    {
        public delegate void CallbackDelegate(string str, object data);

        public CallbackDelegate Callback;
        public ConsoleOutputLevel OutputLevel;
        public object Data;
    }

    public class GameConsole : IGameConsole
    {
        protected IStorage _storage;
        protected Configuration _config;

        protected readonly IDictionary<string, ConsoleCommand> _commands;
        protected readonly IList<string> _executedFiles;
        protected readonly IList<PrintCallback> _printCallbacks;

        public GameConsole()
        {
            _printCallbacks = new List<PrintCallback>();
            _executedFiles = new List<string>();
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

        public virtual ConsoleCommand FindCommand(string command, ConfigFlags flagMask)
        {
            command = command.ToLower();
            return _commands.ContainsKey(command) && (_commands[command].Flags & flagMask) != 0 
                ? _commands[command] 
                : null;
        }

        public virtual ConsoleCommand GetCommand(string command)
        {
            command = command.ToLower();
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

        public virtual void RegisterPrintCallback(ConsoleOutputLevel outputLevel, PrintCallback.CallbackDelegate callback,
            object data)
        {
            _printCallbacks.Add(new PrintCallback()
            {
                Callback = callback,
                Data = data,
                OutputLevel = outputLevel
            });
        }

        public virtual void Print(ConsoleOutputLevel level, string from, string str)
        {
            Base.DbgMessage(from, str, ConsoleColor.DarkYellow);

            for (int i = 0; i < _printCallbacks.Count; i++)
            {
                if (level <= _printCallbacks[i].OutputLevel)
                {
                    _printCallbacks[i].Callback?.Invoke($"[{from}]: {str}", _printCallbacks[i].Data);
                }
            }
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

        protected virtual bool ParseLine(string line, out ConsoleResult result, out ConsoleCommand cmd, 
            out string strCmd)
        {
            line = line.TrimStart();
            var spaceIndex = line.IndexOf(' ');
            strCmd = spaceIndex >= 0 
                ? line.Substring(0, spaceIndex) 
                : line.Substring(0, line.Length);

            cmd = FindCommand(strCmd, ConfigFlags.SERVER);
            if (cmd == null)
            {
                result = null;
                return false;
            }

            var args = "";
            if (spaceIndex > 0 && spaceIndex + 1 < line.Length)
                args = spaceIndex >= 0 ? line.Substring(spaceIndex + 1, line.Length - spaceIndex - 1) : "";

            result = new ConsoleResult(args);
            return true;
        }

        public virtual void ExecuteLine(string line, ConsoleAccessLevel accessLevel = ConsoleAccessLevel.ADMIN)
        {
            ConsoleResult result;
            ConsoleCommand command;
            string strCmd;

            if (ParseLine(line, out result, out command, out strCmd))
            {
            }
            else
            {
                if (string.IsNullOrEmpty(strCmd))
                    return;
                Print(ConsoleOutputLevel.STANDARD, "Console", $"No such command: {strCmd}.");
            }
        }

        public virtual void ExecuteFile(string fileName)
        {
            if (_executedFiles.Any(p => p == fileName))
                return;
            _executedFiles.Add(fileName);

            using (var file = _storage.OpenFile(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                if (file == null)
                {
                    Print(ConsoleOutputLevel.STANDARD, "console", $"failed to open '{fileName}'");
                    return;
                }

                using (var reader = new StreamReader(file))
                {
                    string line;
                    Print(ConsoleOutputLevel.STANDARD, "console", $"executing '{fileName}'");

                    while (!reader.EndOfStream && (line = reader.ReadLine()) != null)
                        ExecuteLine(line);
                }
            }
        }
        
        public virtual void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-f" && i + 1 < args.Length)
                {
                    ExecuteFile(args[i + 1]);
                    i++;
                    continue;
                }

                // search arguments for overrides
                ExecuteLine(args[i]);
            }
        }

        public virtual void RegisterCommand(string command, string formatArguments, ConfigFlags flags, 
            ConsoleCallback callback, object data, string description, 
            ConsoleAccessLevel accessLevel = ConsoleAccessLevel.ADMIN)
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
