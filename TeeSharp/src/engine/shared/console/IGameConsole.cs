using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public delegate void ConsoleCallback(ConsoleResult result, object data);

    public enum AccessLevel
    {
        ADMIN,
        MODERATOR,
        USER
    }

    public interface IGameConsole
    {
        void Init();
        void ExecuteFile(string path);
        void ParseArguments(string[] args);

        void RegisterCommand(string command, string formatArguments, 
            Configuration.ConfigFlags configFlags, ConsoleCallback callback, object data, 
            string description, AccessLevel accessLevel);
    }
}
