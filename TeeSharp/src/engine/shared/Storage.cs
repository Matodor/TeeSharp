using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public class Storage : IStorage
    {
        public const string
            USERDIR = "$USERDIR",
            CURRENTDIR = "$CURRENTDIR";

        protected string _userDirectory;
        protected IList<string> _storagePaths;

        public Storage()
        {
            _storagePaths = new List<string>();
        }

        public virtual void Init(string applicationName)
        {
            _userDirectory = Base.GetStoragePath(applicationName);

            // load paths from storage.cfg
            LoadPaths();

            if (_storagePaths.Count == 0)
            {
                Base.DbgMessage("storage", "using standard paths");
                AddDefaultPaths();
            }
        }

        public virtual Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (!File.Exists(path))
                return null;

            // check all available directories
            for (int i = 0; i < _storagePaths.Count; i++)
            {
                var filePath = Path.Combine(_storagePaths[i], path);
                if (!File.Exists(filePath))
                    continue;

                return File.Open(filePath, mode, access, share);
            }

            return null;
        }

        protected virtual void LoadPaths()
        {
            var path = Path.Combine(Base.GetCurrentWorkingDirectory(), "storage.cfg");
            if (!File.Exists(path))
            {
                Base.DbgMessage("storage", "couldn't open storage.cfg");
                return;
            }

            using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
            using (var reader = new StreamReader(file))
            {
                string line;
                while (!reader.EndOfStream && (line = reader.ReadLine()) != null)
                {
                    if (line.Length > 9 && line.StartsWith("add_path "))
                        AddPath(line.Substring(9));
                }
            }
        }

        protected virtual void AddDefaultPaths()
        {
            AddPath("$USERDIR");
            AddPath("$DATADIR");
            AddPath("$CURRENTDIR");
        }

        protected virtual void AddPath(string path)
        {
            switch (path)
            {
                case USERDIR:
                    if (Directory.Exists(_userDirectory))
                    {
                        _storagePaths.Add(_userDirectory);
                        Base.DbgMessage("storage", $"added path '$USERDIR' ('{_userDirectory}')");
                    }
                    break;
                case CURRENTDIR:
                    break;
                default:
                    if (Directory.Exists(path))
                    {
                        _storagePaths.Add(path);
                        Base.DbgMessage("storage", $"added path '{path}'");
                    }
                    break;
            }
        }
    }
}
