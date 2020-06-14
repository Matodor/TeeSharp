using System;
using System.Collections.Generic;
using System.IO;
using TeeSharp.Core;

namespace TeeSharp.Common.Storage
{
    public class Storage : BaseStorage
    {
        private string _appStoragePath;
        private readonly List<string> _paths;

        public Storage()
        {
            _paths = new List<string>();
        }

        public override bool Init(string appName, StorageType storageType)
        {
            _appStoragePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appName
            );

            if (!Directory.Exists(_appStoragePath))
                Directory.CreateDirectory(_appStoragePath);

            LoadPaths();

            if (_paths.Count == 0)
            {
                Debug.Log("storage", "using standard paths");
                AddDefaultPaths();
            }

            // Furthermore the top entry also defines the save path where
            // all data (settings.cfg, screenshots, ...) are stored.

            var savePath = GetPath(TypeSave, null);
            if (storageType != StorageType.Basic && _paths.Count != 0 && !Directory.Exists(savePath))
            {
                if (storageType == StorageType.Client)
                {
                    Directory.CreateDirectory(GetPath(TypeSave, "screenshots"));
                    Directory.CreateDirectory(GetPath(TypeSave, "screenshots/auto"));
                    Directory.CreateDirectory(GetPath(TypeSave, "maps"));
                    Directory.CreateDirectory(GetPath(TypeSave, "downloadedmaps"));
                }

                Directory.CreateDirectory(GetPath(TypeSave, "dumps"));
                Directory.CreateDirectory(GetPath(TypeSave, "demos"));
                Directory.CreateDirectory(GetPath(TypeSave, "demos/auto"));
            }

            return _paths.Count != 0;
        }

        public override FileStream OpenFile(string fileName, FileAccess fileAccess, int pathIndex = -1)
        {
            if ((fileAccess & FileAccess.Write) != 0)
            {
                var path = GetPath(0, fileName);
                return !File.Exists(path) 
                    ? null 
                    : File.Open(path, FileMode.Open, fileAccess, FileShare.Read)
                    ;
            }

            if (pathIndex >= 0 && pathIndex < _paths.Count)
            {
                var path = GetPath(pathIndex, fileName);
                return !File.Exists(path) 
                    ? null 
                    : File.Open(path, FileMode.Open, fileAccess, FileShare.Read);
            }

            for (var i = 0; i < _paths.Count; i++)
            {
                var path = GetPath(i, fileName);
                if (!File.Exists(path))
                    continue;
                return File.Open(path, FileMode.Open, fileAccess, FileShare.Read);
            }

            return null;
        }

        protected override string GetPath(int pathIndex, string fileName)
        {
            return string.IsNullOrWhiteSpace(fileName) 
                ? _paths[pathIndex] 
                : Path.Combine(_paths[pathIndex], fileName);
        }

        protected override void LoadPaths()
        {
            var cfgPath = Path.Combine(FS.WorkingDirectory(), "storage.cfg");

            if (!File.Exists(cfgPath))
            {
                Debug.Warning("storage", "couldn't open storage.cfg");
                return;
            }

            using (var file = File.Open(cfgPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(file))
                {
                    string currentLine;

                    while (!string.IsNullOrWhiteSpace(currentLine = reader.ReadLine()))
                    {
                        if (currentLine.StartsWith("add_path "))
                            AddPath(currentLine.Replace("add_path ", ""));
                    }
                }

                if (_paths.Count == 0)
                    Debug.Log("storage", "no paths found in storage.cfg");
            }
        }

        protected override void AddPath(string path)
        {
            switch (path)
            {
                case UserDir:
                    if (!Directory.Exists(_appStoragePath))
                        return;
                    _paths.Add(_appStoragePath);
                    Debug.Log("storage", $"added path '{UserDir}' ('{_appStoragePath}')");
                    break;

                case DataDir:
                    var dataPath = Path.Combine(FS.WorkingDirectory(), "data");
                    if (!Directory.Exists(dataPath))
                        return;
                    _paths.Add(dataPath);
                    Debug.Log("storage", $"added path '{DataDir}' ('{dataPath}')");
                    break;

                case CurrentDir:
                    _paths.Add(FS.WorkingDirectory());
                    Debug.Log("storage", $"added path '{CurrentDir}' ('{FS.WorkingDirectory()}')");
                    break;

                default:
                    if (!Directory.Exists(path))
                        return;
                    _paths.Add(path);
                    Debug.Log("storage", $"added path '{path}'");
                    break;
            }
        }
    }
}