using System.Collections.Generic;
using System.IO;
using TeeSharp.Common.Config;
using TeeSharp.Common.Storage;

namespace TeeSharp.Common.Console
{
    public class GameConsole : BaseGameConsole
    {
        private readonly List<string> _alreadyParsed;
        private BaseStorage _storage;
        private BaseConfig _config;

        public GameConsole()
        {
            _alreadyParsed = new List<string>();
            
        }

        public override void Init()
        {
            _storage = Kernel.Get<BaseStorage>();
            _config = Kernel.Get<BaseConfig>();

            foreach (var pair in _config.Variables)
            {
                if (pair.Value is ConfigInt intCfg)
                {

                }
                else if (pair.Value is ConfigString strCfg)
                {
                    
                }
            }
        }

        public override void ExecuteFile(string fileName, bool forcibly = false)
        {
            if (!forcibly && _alreadyParsed.Contains(Path.GetFileName(fileName)))
            {
                return;
            }

            using (var file = _storage.OpenFile(fileName, FileAccess.Read))
            {
                if (file == null)
                {
                    Print(OutputLevel.STANDARD, "console", $"failed to open '{fileName}'");
                    return;
                }

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
        }

        public override void Print(OutputLevel outputLevel, string sys, string format)
        {
            Debug.Log(sys, format);
        }
    }
}