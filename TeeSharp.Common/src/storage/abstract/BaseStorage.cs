using System.IO;
using TeeSharp.Core;

namespace TeeSharp.Common.Storage
{
    public enum StorageType
    {
        Basic = 0,
        Server = 1,
        Client = 2
    }

    public abstract class BaseStorage : BaseInterface
    {
        public const string UserDir = "$USERDIR";
        public const string DataDir = "$DATADIR";
        public const string CurrentDir = "$CURRENTDIR";

        public const int TypeSave = 0;
        public const int TypeAll = -1;

        public abstract bool Init(string appName, StorageType storageType);
        public abstract FileStream OpenFile(string fileName, FileAccess fileAccess, int pathIndex = -1);

        protected abstract string GetPath(int pathIndex, string fileName);
        protected abstract void LoadPaths();
        protected abstract void AddPath(string path);

        protected virtual void AddDefaultPaths()
        {
            AddPath(UserDir);
            AddPath(DataDir);
            AddPath(CurrentDir);
        }
    }
}