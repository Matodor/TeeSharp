using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public delegate void FChatCmdCallback(ChatResult result, object userData);

    public class ChatResult
    {
        public int ClientID { get; }

        private readonly string _argsStored;
        private readonly string[] _args;
        private int _numArgs;

        public ChatResult(int clientID, string args)
        {
            ClientID = clientID;

            _argsStored = args.Trim();
            _numArgs = 0;
            _args = new string[32];
        }

        public int NumArguments()
        {
            return _numArgs;
        }

        public float GetFloat(int index)
        {
            if (index >= _numArgs)
                return 0;
            float f;
            return float.TryParse(_args[index], out f) ? f : 0;
        }

        public int GetInteger(int index)
        {
            if (index >= _numArgs)
                return 0;
            int f;
            return int.TryParse(_args[index], out f) ? f : 0;
        }

        public long GetLong(int index)
        {
            if (index >= _numArgs)
                return 0;
            long f;
            return long.TryParse(_args[index], out f) ? f : 0;
        }

        public string GetString(int index)
        {
            if (index >= _numArgs)
                return "";
            return _args[index];
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
            else if (f == 'l')
            {
                long @out;
                add = long.TryParse(arg, out @out);
            }

            if (add)
                _args[_numArgs++] = arg;
            return add;
        }

        private const string _formatTypes = "sfil";

        public bool ParseArgs(string format)
        {
            if (string.IsNullOrEmpty(format))
                return true;

            format = format.Replace(" ", "");
            if (!format.All(_formatTypes.Contains))
                return false;
            
            string[] args = _argsStored.Split(' ');
            args = args.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            if (args.Length < format.Length)
                return false;

            if (_argsStored.Count(c => c == '"') / 2 == format.Length)
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

            if (format[format.Length - 1] == 's')
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
    }

    public class ChatCommand
    {
        public string Format { get; }
        public FChatCmdCallback Callback { get; }
        public object UserData { get; }

        public ChatCommand(string format, FChatCmdCallback callback, object userData)
        {
            Format = format;
            Callback = callback;
            UserData = userData;
        }   
    }

    public class ChatController
    {
        private readonly Dictionary<string, ChatCommand> _chatCommands;
        private readonly CGameContext _gameContext;

        public ChatController(CGameContext gameContext)
        {
            _gameContext = gameContext;
            _chatCommands = new Dictionary<string, ChatCommand>();
        }

        public void Register(string cmd, string format, FChatCmdCallback callback, object userData = null)
        {
            if (_chatCommands.ContainsKey(cmd))
                return;

            ChatCommand chatCommand = new ChatCommand(format, callback, userData);
            _chatCommands.Add(cmd, chatCommand);
        }

        public bool OnChat(int clientID, string message)
        {
            if (message.Length < 2 || message[0] != '/')
                return false;

            int spaceIndex = message.IndexOf(' ');
            var cmd = spaceIndex >= 0 ? message.Substring(1, spaceIndex - 1) : message.Substring(1, message.Length - 1);

            if (_chatCommands.ContainsKey(cmd))
            {
                string args = "";
                if (spaceIndex + 1 < message.Length)
                    args = spaceIndex >= 0 ? message.Substring(spaceIndex + 1, message.Length - spaceIndex - 1) : "";

                ChatCommand chatCommand = _chatCommands[cmd];
                ChatResult chatResult = new ChatResult(clientID, args);
                if (chatResult.ParseArgs(chatCommand.Format))
                    chatCommand.Callback(chatResult, chatCommand.UserData);
                else
                    _gameContext.SendChatTarget(clientID,
                        _gameContext.m_apPlayers[clientID].Localize("ERROR: Wrong the command format"));
                return true;
            }
            return false;
        }
    }
}
