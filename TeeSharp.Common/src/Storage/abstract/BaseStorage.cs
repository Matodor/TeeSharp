using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Common.Storage
{
    public abstract class BaseStorage : IContainerService
    {
        public const string TokenAppData = "%APP_DATA%";
        public const string TokenCurrentDir = "%CURRENT_DIR%";
        
        protected class StorageConfig
        {
            [JsonProperty("saveDir")]
            public string SaveDirectory { get; set; }

            [JsonProperty("paths")]
            public ICollection<string> Paths { get; set; }
        }
        
        public Container Container { get; set; }

        protected StorageConfig Config { get; set; }
        
        public abstract void Init(string configPath);
        public abstract bool TryOpen(string fileName, FileAccess access);
        public abstract void LoadConfig(string configPath);
        public abstract void AddPath(string path);
        public abstract string FormatPath(string path);
        
        protected abstract void Setup();
        protected abstract void LoadDefaultPaths();
    }
}