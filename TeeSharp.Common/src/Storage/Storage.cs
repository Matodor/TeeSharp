using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Serilog;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Common.Storage
{
    public class Storage : BaseStorage
    {
        public override void Init(string configPath)
        {
            Config = new StorageConfig
            {
                Paths = new List<string>(),
                SaveDirectory = null,
            };

            LoadConfig(configPath);
            Setup();
        }

        public override bool TryOpen(string fileName, FileAccess access)
        {
            throw new System.NotImplementedException();
        }

        public override void LoadConfig(string configPath)
        {
            if (!File.Exists(configPath))
                return;

            StorageConfig config;

            try
            {
                var json = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<StorageConfig>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            if (config == null)
                return;

            if (!string.IsNullOrWhiteSpace(config.SaveDirectory))
                Config.SaveDirectory = FormatPath(config.SaveDirectory);

            if (config.Paths != null && config.Paths.Count > 0)
                foreach (var path in config.Paths)
                    AddPath(path);
        }

        public override void AddPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            path = FormatPath(path);

            if (Config.Paths.Contains(path))
                return;

            Config.Paths.Add(path);
            Log.Information($"[storage] Add path: {path}");
        }

        public override string FormatPath(string path)
        {
            if (path == null)
                return null;

            path = path.Replace(TokenAppData, FSHelper.AppDataPath());
            path = path.Replace(TokenCurrentDir, FSHelper.WorkingPath());

            return Path.GetFullPath(path);
        }

        protected override void Setup()
        {
            if (string.IsNullOrWhiteSpace(Config.SaveDirectory))
                Config.SaveDirectory = FormatPath(FSHelper.AppDataPath("TeeSharp"));

            if (Config.Paths == null || Config.Paths.Count == 0)
            {
                Log.Information("[storage] Config `paths` is empty, default paths will be used");
                LoadDefaultPaths();
            }

            if (File.Exists(Config.SaveDirectory))
            {
                Log.Error("[storage] Config `saveDir` is existing file, saving files won't work");
                Config.SaveDirectory = null;
            }
            else
            {
                Log.Information($"[storage] Using save path at: {Config.SaveDirectory}");
                Directory.CreateDirectory(Config.SaveDirectory);
            }
        }

        protected override void LoadDefaultPaths()
        {
            AddPath(TokenAppData + "/TeeSharp");
            AddPath(TokenCurrentDir + "/data");
            AddPath(TokenCurrentDir);
        }
    }
}