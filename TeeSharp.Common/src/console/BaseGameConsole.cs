using System;
using TeeSharp.Common.Config;

namespace TeeSharp.Common.Console
{
    public enum OutputLevel
    {
        STANDARD = 0,
        ADDINFO = 1,
        DEBUG = 2,
    }

    public delegate void ConsoleCallback(ConsoleResult result, object data);
    public delegate void PrintCallback(string message, object data);

    public class PrintCallbackInfo
    {
        public OutputLevel OutputLevel;
        public PrintCallback Callback;
        public object Data;
    }

    public abstract class BaseGameConsole : BaseInterface
    {


        public abstract void Init();
        public abstract void RegisterCommand(string cmd, string format, ConsoleCallback callback, ConfigFlags flags,
            string description, object data = null);
        public abstract void ExecuteFile(string fileName, bool forcibly = false);
        public abstract void ParseArguments(string[] args);
        public abstract void ExecuteLine(string line);
        public abstract void Print(OutputLevel outputLevel, string sys, string format);
        public abstract PrintCallbackInfo RegisterPrintCallback(OutputLevel outputLevel, 
            PrintCallback callback, object data = null);
        public abstract ConsoleCommand FindCommand(string cmd, ConfigFlags mask);

        protected abstract void StrVariableCommand(ConsoleResult result, object data);
        protected abstract void IntVariableCommand(ConsoleResult result, object data);

        protected abstract bool ParseLine(string line, out ConsoleResult result,
            out ConsoleCommand command, out string parsedCmd);
    }
}