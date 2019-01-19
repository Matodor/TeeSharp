using System;
using System.Collections;
using System.Collections.Generic;
using TeeSharp.Common.Config;
using TeeSharp.Common.Storage;
using TeeSharp.Core;

namespace TeeSharp.Common.Console
{
    public enum OutputLevel
    {
        Standard = 0,
        AddInfo,
        Debug,
    }

    public delegate void PrintCallback(string message, object data);

    public class PrintCallbackInfo
    {
        public OutputLevel OutputLevel { get; set; }
        public PrintCallback Callback { get; set; }
        public object Data { get; set; }
    }

    public abstract class BaseGameConsole : BaseInterface, IEnumerable<KeyValuePair<string, ConsoleCommand>>
    {
        public abstract event Action<ConsoleCommand> CommandAdded;
        
        public abstract ConsoleCommand this[string command] { get; }

        protected virtual BaseStorage Storage { get; set; }
        protected virtual BaseConfig Config { get; set; }

        protected virtual IList<PrintCallbackInfo> PrintCallbacks { get; set; }
        protected virtual IList<string> ExecutedFiles { get; set; }
        protected virtual IDictionary<string, ConsoleCommand> Commands { get; set; }

        public abstract void Init();

        public abstract void ExecuteFile(string fileName, bool forcibly = false);
        public abstract void ParseArguments(string[] args);
        public abstract void ExecuteLine(string line, int accessLevel, int clientId = -1);
        public abstract void Print(OutputLevel outputLevel, string sys, string format);
        public abstract ConsoleCommand FindCommand(string cmd, ConfigFlags mask);
        public abstract void SetAccessLevel(int accessLevel, params string[] commands);

        public abstract void AddCommand(string cmd, string format, string description,
            ConfigFlags flags, CommandCallback callback, object data = null);

        public abstract IEnumerable<KeyValuePair<string, ConsoleCommand>> GetCommands(
            int accessLevel, ConfigFlags flags = ConfigFlags.All);

        public abstract PrintCallbackInfo RegisterPrintCallback(OutputLevel outputLevel,
            PrintCallback callback, object data = null);

        protected abstract void StrVariableCommand(ConsoleCommandResult commandResult, int clientId, ref object data);
        protected abstract void IntVariableCommand(ConsoleCommandResult commandResult, int clientId, ref object data);

        protected abstract bool ParseLine(string line, out string arguments,
            out ConsoleCommand command, out string parsedCmd);

        public abstract IEnumerator<KeyValuePair<string, ConsoleCommand>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}