using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public delegate void FConsoleCallback(CConsoleResult result, object data);
    public delegate void FChainCommandCallback(CConsoleResult result, object data, FConsoleCallback callback, object callbackUserData);
    public delegate void FPrintCallback(string str, object data);

    public class CChain
    {
        public FChainCommandCallback ChainCallback { get; set; }
        public FConsoleCallback Callback { get; set; }
        public object CallbackUserData { get; set; }
        public object UserData { get; set; }
    }

    public class CConsoleCommand
    {
        public string Format { get; }
        public FConsoleCallback Callback { get; set; }
        public object UserData { get; set; }
        public int AccessLevel { get; }
        public int FlagMask { get; }
        public string Help { get; }
        public string Name { get; }

        public CConsoleCommand(string format, FConsoleCallback callback, object userData, int accessLevel, int flag, string help, string name)
        {
            Name = name;
            Help = help;
            FlagMask = flag;
            Format = format;
            Callback = callback;
            UserData = userData;
            AccessLevel = accessLevel;
        }
    }

    public class PrintCallBack
    {
        public int OutputLevel { get; set; }
        public FPrintCallback PrintCallback { get; set; }
        public object PrintCallbackUserdata { get; set; }
    }

    public class CConsoleResult
    {
        private readonly string _argsStored;
        private readonly string[] _args;
        private int _numArgs;

        public CConsoleResult(string args)
        {
            _argsStored = args.Trim();
            _numArgs = 0;
            _args = new string[32];
        }

        public int NumArguments()
        {
            return _numArgs;
        }

        private bool AddArgument(string arg, char f)
        {
            bool add = true;
            if (f == 'f')
            {
                float @out;
                add = float.TryParse(arg, out @out);
            }
            else if (f == 'i')
            {
                int @out;
                add = int.TryParse(arg, out @out);
            }

            if (add)
                _args[_numArgs++] = arg;
            return add;
        }

        private const string _formatTypes = "sfi";

        public bool ParseArgs(string format)
        {
            if (format == null)
                format = "";

            if (format.Length >= 1 && format[0] == '?' && string.IsNullOrEmpty(_argsStored))
                return true;

            format = format.Replace(" ", "").Replace("?", "").Replace("r", "s");
            if (!format.All(_formatTypes.Contains))
                return false;

            string[] args = _argsStored.Split(' ');
            args = args.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            if (args.Length < format.Length)
                return false;

            if (_argsStored.Count(c => c == '"') / 2 == format.Length && format.Length != 0)
            {
                int i1 = 0;
                for (int i = 0; i < format.Length; i++)
                {
                    i1 = _argsStored.IndexOf('"', i1);
                    var i2 = _argsStored.IndexOf('"', i1 + 1);
                    if (!AddArgument(_argsStored.Substring(i1 + 1, i2 - (i1 + 1)), format[i]))
                        return false;
                    i1 = i2 + 1;
                }
                return true;
            }

            if (format.Length > 0 && format[format.Length - 1] == 's')
            {
                for (int i = 0; i < format.Length - 1; i++)
                    if (!AddArgument(args[i], format[i]))
                        return false;

                StringBuilder builder = new StringBuilder("");
                for (int i = format.Length - 1; i < args.Length; i++)
                    builder.Append(args[i] + " ");
                builder.Remove(builder.Length - 1, 1);

                if (!string.IsNullOrEmpty(builder.ToString()))
                    if (!AddArgument(builder.ToString(), 's'))
                        return false;
                return true;
            }
            int z = 0;
            foreach (string arg in args)
                if (!AddArgument(arg, z < format.Length ? format[z++] : 's'))
                    return false;
            return true;
        }

        public int GetInteger(uint index)
        {
            return int.Parse(_args[index]);
        }

        public float GetFloat(uint index)
        {
            return float.Parse(_args[index]);
        }

        public string GetString(uint index)
        {
            return _args[index];
        }
    }

    public partial class CConsole : IConsole
    {
        private readonly PrintCallBack[] _aPrintCB = new PrintCallBack[MAX_PRINT_CB];
        private int _numPrintCB;

        private readonly List<string> _execFiles = new List<string>();
        private IStorage _storage;
        private readonly Dictionary<string, CConsoleCommand> _consoleCommands = new Dictionary<string, CConsoleCommand>(); 

        public CConsole()
        {
        }

        public IEnumerator<KeyValuePair<string, CConsoleCommand>> GetEnumerator(int accessLevel, int flag)
        {
            return _consoleCommands.Where(c => c.Value.AccessLevel >= accessLevel && (c.Value.FlagMask & flag) != 0).GetEnumerator();
        }

        public override void Init()
        {
            if (_storage == null)
                _storage = Kernel.RequestInterface<IStorage>();

            _numPrintCB = 0;
            for (int i = 0; i < _aPrintCB.Length; i++)
                _aPrintCB[i] = new PrintCallBack();

            // register some basic commands
            /*Register("echo", "r", CConfiguration.CFGFLAG_SERVER | CConfiguration.CFGFLAG_CLIENT, Con_Echo, this,
                "Echo the text");
            Register("exec", "r", CConfiguration.CFGFLAG_SERVER | CConfiguration.CFGFLAG_CLIENT, Con_Exec, this,
                "Execute the specified file");

            Register("mod_command", "s?i", CConfiguration.CFGFLAG_SERVER, ConModCommandAccess, this,
                "Specify command accessibility for moderators");
            Register("mod_status", "", CConfiguration.CFGFLAG_SERVER, ConModCommandStatus, this,
                "List all commands which are accessible for moderators");*/

            foreach (var variable in CConfiguration.Variables)
            {
                if (variable.Value.GetType() == typeof(ConfigInt))
                {
                    ConfigInt Data = (ConfigInt)variable.Value;
                    Register(Data.ScriptName, "?i", Data.Flags, IntVariableCommand, Data, Data.Desc);
                }
                else if (variable.Value.GetType() == typeof(ConfigStr))
                {
                    ConfigStr Data = (ConfigStr)variable.Value;
                    Register(Data.ScriptName, "?s", Data.Flags, StrVariableCommand, Data, Data.Desc);
                }
            }
        }

        private void StrVariableCommand(CConsoleResult result, object data)
        {
            ConfigStr pData = (ConfigStr)data;

            if (result.NumArguments() != 0)
            {
                string pString = result.GetString(0);
                pData.Default = pString;
            }
            else
            {
                string aBuf = string.Format("Value: {0}", pData.Default);
                Print(OUTPUT_LEVEL_STANDARD, "Console", aBuf);
            }
        }

        private void IntVariableCommand(CConsoleResult result, object data)
        {
            ConfigInt pData = (ConfigInt)data;

            if (result.NumArguments() != 0)
            {
                int Val = result.GetInteger(0);
                pData.Default = CMath.clamp(Val, pData.Min, pData.Max);
            }
            else
            {
                string aBuf = string.Format("Value: {0}", pData.Default);
                Print(OUTPUT_LEVEL_STANDARD, "Console", aBuf);
            }
        }

        public static IConsole CreateConsole()
        {
            return new CConsole();
        }

        public override void ExecuteFile(string fileName)
        {
            // make sure that this isn't being executed already
            if (_execFiles.FirstOrDefault(f => f == fileName) != null)
            {
                return;
            }

            // exec the file
            var fileStream = _storage.OpenFile(fileName, CSystem.IOFLAG_READ, IStorage.TYPE_ALL);

            string aBuf;
            if (fileStream != null)
            {
                string pLine;
                TextReader LineReader = new StreamReader(fileStream);

                aBuf = string.Format("executing '{0}'", fileName);
                Print(OUTPUT_LEVEL_STANDARD, "console", aBuf);

                while ((pLine = LineReader.ReadLine()) != null)
                    ExecuteLine(pLine, ACCESS_LEVEL_ADMIN);
                CSystem.io_close(fileStream);
            }
            else
            {
                aBuf = string.Format("failed to open '{0}'", fileName);
                Print(OUTPUT_LEVEL_STANDARD, "console", aBuf);
            }
        }

        private bool ParseLine(string line, out CConsoleResult result, out CConsoleCommand command, out string cmd)
        {
            line = line.TrimStart();
            var spaceIndex = line.IndexOf(' ');
            cmd = spaceIndex >= 0 ? line.Substring(0, spaceIndex) : line.Substring(0, line.Length);

            command = FindCommand(cmd, CConfiguration.CFGFLAG_SERVER);
            if (command == null)
            {
                result = null;
                return false;
            }

            var args = "";
            if (spaceIndex > 0 && spaceIndex + 1 < line.Length)
                args = spaceIndex >= 0 ? line.Substring(spaceIndex + 1, line.Length - spaceIndex - 1) : "";

            result = new CConsoleResult(args);
            return true;
        }

        private CConsoleCommand FindCommand(string command, int flagMask)
        {
            command = command.ToLower();
            if (_consoleCommands.ContainsKey(command) && (_consoleCommands[command].FlagMask & flagMask) != 0)
                return _consoleCommands[command];
            return null;
        }

        public override void ExecuteLine(string line, int accessLevel)
        {
            CConsoleResult result;
            CConsoleCommand command;
            string strCmd;

            if (ParseLine(line, out result, out command, out strCmd))
            {
                    if (command.AccessLevel >= accessLevel)
                    {
                        if (!result.ParseArgs(command.Format))
                        {
                            string aBuf = string.Format("Invalid arguments... Usage: {0} {1}", strCmd, command.Format);
                            Print(OUTPUT_LEVEL_STANDARD, "Console", aBuf);
                        }
                        else
                            command.Callback(result, command.UserData);
                    }
                    else
                    {
                        string aBuf = string.Format("Access for command {0} denied.", strCmd);
                        Print(OUTPUT_LEVEL_STANDARD, "Console", aBuf);
                    }
            }
            else
            {
                if (string.IsNullOrEmpty(strCmd))
                    return;

                string aBuf = string.Format("No such command: {0}.", strCmd);
                Print(OUTPUT_LEVEL_STANDARD, "Console", aBuf);
            }
        }

        public override void ParseArguments(params string[] ppArguments)
        {
            for (int i = 0; i < ppArguments.Length; i++)
            {
                // check for scripts to execute
                if (ppArguments[i][0] == '-' && ppArguments[i][1] == 'f')
                {
                    if (ppArguments.Length - i > 1)
                        ExecuteFile(ppArguments[i + 1]);
                    i++;
                }
                else if (CSystem.str_comp("-s", ppArguments[i]) == 0 || CSystem.str_comp("--silent", ppArguments[i]) == 0)
                {
                    // skip silent param
                    continue;
                }
                else
                {
                    // search arguments for overrides
                    ExecuteLine(ppArguments[i], ACCESS_LEVEL_ADMIN);
                }
            }
        }

        public override void Register(string cmd, string format, int flags, FConsoleCallback callback,
            object data, string help, int accessLevel = ACCESS_LEVEL_ADMIN)
        {
            CConsoleCommand command = new CConsoleCommand(format, callback, data, accessLevel, flags, help, cmd);
            if (!_consoleCommands.ContainsKey(cmd))
                _consoleCommands.Add(cmd, command);
        }

        public override void Chain(string name, FChainCommandCallback chainFunc, object user)
        {
            CConsoleCommand command = FindCommand(name, CConfiguration.CFGFLAG_SERVER);

            if (command == null)
            {
                string aBuf = string.Format("failed to chain '{0}'", name);
                Print(OUTPUT_LEVEL_DEBUG, "console", aBuf);
                return;
            }

            CChain pChainInfo = new CChain();

            // store info
            pChainInfo.ChainCallback = chainFunc;
            pChainInfo.UserData = user;
            pChainInfo.Callback = command.Callback;
            pChainInfo.CallbackUserData = command.UserData;

            // chain
            command.Callback = Con_Chain;
            command.UserData = pChainInfo;
        }

        public static void Con_Chain(CConsoleResult result, object data)
        {
            CChain chain = (CChain)data;
            chain.ChainCallback(result, data, chain.Callback, chain.CallbackUserData);
        }

        public override int RegisterPrintCallback(int outputLevel, FPrintCallback callback, object data)
        {
            if (_numPrintCB == MAX_PRINT_CB)
                return -1;

            _aPrintCB[_numPrintCB].OutputLevel = CMath.clamp(outputLevel, OUTPUT_LEVEL_STANDARD, OUTPUT_LEVEL_DEBUG);
            _aPrintCB[_numPrintCB].PrintCallback = callback;
            _aPrintCB[_numPrintCB].PrintCallbackUserdata = data;
            return _numPrintCB++;
        }

        public override void Print(int level, string from, string str)
        {
            CSystem.dbg_msg_clr(from, "{0}", ConsoleColor.DarkYellow, str);

            for (int i = 0; i < _numPrintCB; ++i)
            {
                if (level <= _aPrintCB[i].OutputLevel && _aPrintCB[i].PrintCallback != null)
                {
                    string aBuf = string.Format("[{0}]: {1}", from, str);
                    _aPrintCB[i].PrintCallback(aBuf, _aPrintCB[i].PrintCallbackUserdata);
                }
            }
        }
    }
}
