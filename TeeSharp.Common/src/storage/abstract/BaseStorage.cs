﻿using System.IO;
using TeeSharp.Core;

namespace TeeSharp.Common.Storage
{
    public enum StorageType
    {
        BASIC = 0,
        SERVER = 1,
        CLIENT = 2
    }

    public abstract class BaseStorage : BaseInterface
    {
        public const int
            TYPE_SAVE = 0,
            TYPE_ALL = -1;

        public abstract bool Init(string appName, StorageType storageType);
        public abstract FileStream OpenFile(string fileName, FileAccess fileAccess, int pathIndex = -1);

        protected abstract string GetPath(int pathIndex, string fileName);
        protected abstract void LoadPaths();
        protected abstract void AddPath(string path);

        protected virtual void AddDefaultPaths()
        {
            AddPath("$USERDIR");
            AddPath("$DATADIR");
            AddPath("$CURRENTDIR");
        }
    }
}