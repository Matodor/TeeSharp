using System;
using System.Collections.Generic;
using System.IO;
using TeeSharp.Common.Config;
using TeeSharp.Common.Storage;

namespace TeeSharp.Common.Console
{
    public class GameConsole : BaseGameConsole
    {
        protected BaseStorage Storage { get; private set; }
        protected BaseConfig Config { get; private set; }

        protected virtual IList<PrintCallbackInfo> PrintCallbacks { get; set; }
        protected virtual IList<string> ExecutedFiles { get; set; }
        protected virtual IDictionary<string, ConsoleCommand> Commands { get; set; }

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

            foreach (var pair in Config.Variables)
            {
                if (pair.Value is ConfigInt intCfg)
                {
                    RegisterCommand(intCfg.ConsoleCommand, "?i", IntVariableCommand, 
                        intCfg.Flags, intCfg.Description, intCfg);
                }
                else if (pair.Value is ConfigString strCfg)
                {
                    RegisterCommand(strCfg.ConsoleCommand, "?s", StrVariableCommand,
                        strCfg.Flags, strCfg.Description, strCfg);
                }
            }
        }

        public override void RegisterCommand(string cmd, string format, ConsoleCallback callback, 
            ConfigFlags flags, string description, object data = null)
        {
            if (Commands.ContainsKey(cmd))
                return;

            cmd = cmd.Trim();
            format = format.Trim().Replace("??", "?");

            Commands.Add(cmd, new ConsoleCommand(cmd, format, flags,
                description, callback, data));
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

        protected override void StrVariableCommand(ConsoleResult result, object data)
        {
            if (result.NumArguments != 0)
                ((ConfigString) data).Value = (string) result[0];
            else
                Print(OutputLevel.STANDARD, "console", $"Value: {((ConfigString) data).Value}");
        }

        protected override void IntVariableCommand(ConsoleResult result, object data)
        {
            if (result.NumArguments != 0)
                ((ConfigInt) data).Value = (int) result[0];
            else
                Print(OutputLevel.STANDARD, "console", $"Value: {((ConfigInt) data).Value}");
        }

        protected override bool ParseLine(string line, out ConsoleResult result, out ConsoleCommand command, out string parsedCmd)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                result = null;
                command = null;
                parsedCmd = null;
                return false;
            }

            line = line.TrimStart();
            var space = line.IndexOf(' ');
            parsedCmd = space > 0 ? line.Substring(0, space) : line;

            if (!Commands.TryGetValue(parsedCmd, out command))
            {
                result = null;
                return false;
            }

            var args = string.Empty;
            if (space > 0 && space + 1 < line.Length)
                args = line.Substring(space + 1);

            result = new ConsoleResult(args);
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
                    Print(OutputLevel.STANDARD, "console", $"failed to open '{fileName}'");
                    return;
                }

                ExecutedFiles.Add(fileName);
                using (var reader = new StreamReader(file))
                {
                    Print(OutputLevel.STANDARD, "console", $"executing '{fileName}'");
                    string currentLine;

                    while (!string.IsNullOrWhiteSpace(currentLine = reader.ReadLine()))
                        ExecuteLine(currentLine);
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
                    ExecuteLine(args[i]);
                }
            }
        }

        public override void ExecuteLine(string line)
        {
            if (ParseLine(line, out var result, out var command, out var parsedCmd))
            {
                if (result.ParseArguments(command.Format))
                {
                    command.Callback(result, command.Data);
                }
                else
                {
                    Print(OutputLevel.STANDARD, "console", 
                        $"Invalid arguments... Usage: {command.Cmd} {command.Format}");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(parsedCmd))
                    return;

                Print(OutputLevel.STANDARD, "console", $"No such command: {parsedCmd}.");
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
    }
}