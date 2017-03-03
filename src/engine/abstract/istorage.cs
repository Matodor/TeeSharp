using System;
using System.IO;

namespace Teecsharp
{
    public abstract class IStorage : IInterface
    {
        public const int
            TYPE_SAVE = 0,
            TYPE_ALL = -1,
            STORAGETYPE_BASIC = 0,
            STORAGETYPE_SERVER = 1,
            STORAGETYPE_CLIENT = 2;


        public abstract void ListDirectory(int Type, string pPath,
            Func<string, int, int, object, int> pfnCallback, object pUser);
        public abstract FileStream OpenFile(string pFilename, int Flags, int Type);
        public abstract bool FindFile(string pFilename, string pPath, int Type, ref string pFilePath);
        public abstract bool RemoveFile(string pFilename, int Type);
        public abstract bool RenameFile(string pOldFilename, string pNewFilename, int Type);
        public abstract bool CreateFolder(string pFoldername, int Type);
        public abstract string GetCompletePath(int Type, string pDir);
    }
}
