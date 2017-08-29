using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public delegate void ConsoleCallback(ConsoleResult result, object data);

    public enum ConsoleOutputLevel
    {
        STANDARD = 0,
        ADDINFO = 1,
        DEBUG = 2,
    }

    public enum ConsoleAccessLevel
    {
        ADMIN,
        MODERATOR,
        USER
    }

    public interface IGameConsole
    {
        void Init();
        void ExecuteLine(string line, ConsoleAccessLevel accessLevel = ConsoleAccessLevel.ADMIN);
        void ExecuteFile(string path);
        void ParseArguments(string[] args);

        void RegisterPrintCallback(ConsoleOutputLevel outputLevel, PrintCallback.CallbackDelegate callback,
            object data);
        void Print(ConsoleOutputLevel level, string from, string str);
        void OnExecuteCommand(string command, ConsoleCallback callback);
        void RegisterCommand(string command, string formatArguments, 
            ConfigFlags configFlags, ConsoleCallback callback, object data, 
            string description, ConsoleAccessLevel accessLevel = ConsoleAccessLevel.ADMIN);

        ConsoleCommand FindCommand(string command, ConfigFlags flagMask);
        ConsoleCommand GetCommand(string command);
    }
}
