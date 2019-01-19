using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeeSharp.Common.Config;
using TeeSharp.Common.Storage;
using TeeSharp.Core;

namespace TeeSharp.Common.Console
{
    public class GameConsole : BaseGameConsole
    {
        public override event Action<ConsoleCommand> CommandAdded;

        public override ConsoleCommand this[string command]
        {
            get
            {
                if (Commands.ContainsKey(command))
                    return Commands[command];
                throw new Exception("Command not found");
            }
        }

        public GameConsole()
        {
            PrintCallbacks = new List<PrintCallbackInfo>();
            ExecutedFiles = new List<string>();
            Commands = new Dictionary<string, ConsoleCommand>();
        }

        public override void Init()
        {
            Storage = Kernel.Get<BaseStorage>();
            Config = Kernel.Get<BaseConfig>();

            AddCommand("echo", "r", "Echo the text", ConfigFlags.Server | ConfigFlags.Client, ConsoleEchoText);
            AddCommand("exec", "r", "Execute the specified file", ConfigFlags.Server | ConfigFlags.Client, ConsoleExec);
            AddCommand("toggle", "sii", "Toggle config value", ConfigFlags.Server | ConfigFlags.Client, ConsoleToggle);

            foreach (var pair in Config)
            {
                if (pair.Value is ConfigInt intCfg)
                {
                    AddCommand(
                        intCfg.ConsoleCommand, "?i", 
                        intCfg.Description, 
                        intCfg.Flags, 
                        IntVariableCommand, 
                        intCfg);
                }
                else if (pair.Value is ConfigString strCfg)
                {
                    AddCommand(strCfg.ConsoleCommand, "?s", 
                        strCfg.Description, 
                        strCfg.Flags, 
                        StrVariableCommand,
                        strCfg);
                }
            }
        }

        protected virtual void ConsoleToggle(ConsoleCommandResult commandresult, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleExec(ConsoleCommandResult commandresult, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleEchoText(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        public override void SetAccessLevel(int accessLevel, params string[] commands)
        {
            for (var i = 0; i < commands.Length; i++)
            {
                Commands[commands[i]].AccessLevel = accessLevel;
            }
        }

        public override void AddCommand(string cmd, string format, string description, ConfigFlags flags, CommandCallback callback, object data = null)
        {
            if (Commands.ContainsKey(cmd))
            {
                Debug.Warning("console", $"Command {cmd} already exist");
                return;
            }

            var command = new ConsoleCommand(cmd, format, description, flags, data);
            command.Executed += callback;
            Commands.Add(cmd, command);
            CommandAdded?.Invoke(command);
        }

        public override IEnumerable<KeyValuePair<string, ConsoleCommand>> GetCommands(int accessLevel, ConfigFlags flags = ConfigFlags.All)
        {
            return Commands.Where(pair => pair.Value.AccessLevel <= accessLevel &&
                                          pair.Value.Flags.HasFlag(flags));
        }

        public override PrintCallbackInfo RegisterPrintCallback(OutputLevel outputLevel, 
            PrintCallback callback, object data = null)
        {
            var info = new PrintCallbackInfo
            {
                OutputLevel = outputLevel,
                Callback = callback,
                Data = data
            };

            PrintCallbacks.Add(info);
            return info;
        }

        public override ConsoleCommand FindCommand(string cmd, ConfigFlags mask)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                return null;

            cmd = cmd.ToLower();
            return Commands.ContainsKey(cmd) && (Commands[cmd].Flags & mask) != 0
                ? Commands[cmd]
                : null;
        }

        protected override void StrVariableCommand(ConsoleCommandResult commandResult, int clientId, ref object data)
        {
            if (commandResult.ArgumentsCount != 0)
                ((ConfigString) data).Value = (string) commandResult[0];
            else
                Print(OutputLevel.Standard, "console", $"Value: {((ConfigString) data).Value}");
        }

        protected override void IntVariableCommand(ConsoleCommandResult commandResult, int clientId, ref object data)
        {
            if (commandResult.ArgumentsCount != 0)
                ((ConfigInt) data).Value = (int) commandResult[0];
            else
                Print(OutputLevel.Standard, "console", $"Value: {((ConfigInt) data).Value}");
        }

        protected override bool ParseLine(string line, out string arguments, out ConsoleCommand command, out string parsedCmd)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                arguments = null;
                command = null;
                parsedCmd = null;
                return false;
            }

            line = line.TrimStart();
            var space = line.IndexOf(' ');
            parsedCmd = space > 0 ? line.Substring(0, space) : line;

            if (!Commands.TryGetValue(parsedCmd, out command))
            {
                arguments = null;
                return false;
            }

            var args = string.Empty;
            if (space > 0 && space + 1 < line.Length)
                args = line.Substring(space + 1);

            arguments = args;
            return true;
        }

        public override void ExecuteFile(string fileName, bool forcibly = false)
        {
            if (!forcibly && ExecutedFiles.Contains(Path.GetFileName(fileName)))
            {
                return;
            }

            using (var file = Storage.OpenFile(fileName, FileAccess.Read))
            {
                if (file == null)
                {
                    Print(OutputLevel.Standard, "console", $"failed to open '{fileName}'");
                    return;
                }

                ExecutedFiles.Add(fileName);
                using (var reader = new StreamReader(file))
                {
                    Print(OutputLevel.Standard, "console", $"executing '{fileName}'");
                    string currentLine;

                    while (!reader.EndOfStream && (currentLine = reader.ReadLine()) != null)
                        ExecuteLine(currentLine, -1);
                }
            }
        }

        public override void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-f")
                {
                    if (i + 1 < args.Length)
                        ExecuteFile(args[i + 1]);
                    i++;
                }
                else
                {
                    ExecuteLine(args[i], -1);
                }
            }
        }

        public override void ExecuteLine(string line, int accessLevel, int clientId = -1)
        {
            if (string.IsNullOrEmpty(line))
                return;

            // TODO separeted commands
            if (ParseLine(line, out var arguments, out var command, out var parsedCmd))
            {
                if (ConsoleCommandResult.Parse(arguments, command.ParametersFormat, out var result))
                {
                    if (accessLevel == -1 || accessLevel >= command.AccessLevel)
                        command.Invoke(result, clientId);
                    else
                    {
                        Print(OutputLevel.Standard, "console", $"Insufficient access level for execute command '{line}'");
                    }
                }
                else
                {
                    Print(OutputLevel.Standard, "console", 
                        $"Invalid arguments... Usage: {command.Cmd} {command.ParametersFormat}");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(parsedCmd))
                    return;

                Print(OutputLevel.Standard, "console", $"No such command: {parsedCmd}.");
            }
        }

        public override void Print(OutputLevel outputLevel, string sys, string format)
        {
            Debug.Log(sys, format);

            for (var i = 0; i < PrintCallbacks.Count; i++)
            {
                if (PrintCallbacks[i] != null &&
                    PrintCallbacks[i].OutputLevel >= outputLevel)
                {
                    PrintCallbacks[i]?.Callback($"[{sys}]: {format}", PrintCallbacks[i].Data);
                }
            }
        }

        public override IEnumerator<KeyValuePair<string, ConsoleCommand>> GetEnumerator()
        {
            return Commands.GetEnumerator();
        }
    }
}