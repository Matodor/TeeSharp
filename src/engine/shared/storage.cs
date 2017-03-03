using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    class CStorage : IStorage
    {
        const int
            MAX_PATHS = 16,
            MAX_PATH_LENGTH = 512;

        string[] m_aaStoragePaths = new string[MAX_PATHS];
        int m_NumPaths;
        string m_aDatadir;
        string m_aUserdir;
        string m_aCurrentdir;

        public CStorage()
        {
            m_NumPaths = 0;
            m_aDatadir = null;
            m_aUserdir = null;
        }

        public static IStorage CreateStorage(string pApplicationName, int StorageType, params string[] ppArguments)
        {
            return Create(pApplicationName, StorageType, ppArguments);
        }

        static IStorage Create(string pApplicationName, int StorageType, params string[] ppArguments)
        {
            CStorage p = new CStorage();
            if (p.Init(pApplicationName, StorageType, ppArguments) != 0)
            {
                CSystem.dbg_msg("storage", "initialisation failed");
                p = null;
            }
            return p;
        }

        int Init(string pApplicationName, int StorageType, params string[] ppArguments)
        {
            // get userdir
            m_aUserdir = CSystem.fs_storage_path(pApplicationName);

            // get datadir
            if (ppArguments.Length > 0)
                FindDatadir(ppArguments[0]);

            // get currentdir
            m_aCurrentdir = CSystem.fs_getcwd();

            // load paths from storage.cfg
            if (ppArguments.Length > 0)
                LoadPaths(ppArguments[0]);

            if (m_NumPaths == 0)
            {
                CSystem.dbg_msg("storage", "using standard paths");
                AddDefaultPaths();
            }

            // add save directories
            if (StorageType != STORAGETYPE_BASIC && m_NumPaths != 0 &&
                (!string.IsNullOrEmpty(m_aaStoragePaths[TYPE_SAVE]) || CSystem.fs_makedir(m_aaStoragePaths[TYPE_SAVE])))
            {
                if (StorageType == STORAGETYPE_CLIENT)
                {
                    CSystem.fs_makedir(GetPath(TYPE_SAVE, "screenshots"));
                    CSystem.fs_makedir(GetPath(TYPE_SAVE, "screenshots/auto"));
                    CSystem.fs_makedir(GetPath(TYPE_SAVE, "maps"));
                    CSystem.fs_makedir(GetPath(TYPE_SAVE, "downloadedmaps"));
                }
                CSystem.fs_makedir(GetPath(TYPE_SAVE, "dumps"));
                CSystem.fs_makedir(GetPath(TYPE_SAVE, "demos"));
                CSystem.fs_makedir(GetPath(TYPE_SAVE, "demos/auto"));
            }

            return m_NumPaths != 0 ? 0 : 1;
        }

        void LoadPaths(string pArgv0)
        {
            string cDir = CSystem.fs_getcwd();
            // check current directory
            FileStream File = CSystem.io_open(cDir + "/storage.cfg", CSystem.IOFLAG_READ);
            if (File == null)
            {
                CSystem.dbg_msg("storage", "couldn't open storage.cfg");
                return;
            }

            TextReader LineReader = new StreamReader(File);

            string pLine;
            while ((pLine = LineReader.ReadLine()) != null)
            {
                if (pLine.Length > 9 && pLine.StartsWith("add_path "))
                    AddPath(pLine.Substring(9));
            }

            CSystem.io_close(File);

            if (m_NumPaths == 0)
                CSystem.dbg_msg("storage", "no paths found in storage.cfg");
        }

        string GetPath(int Type, string pDir)
        {
            var pBuffer = string.Format("{0}{1}{2}", m_aaStoragePaths[Type],
                string.IsNullOrEmpty(m_aaStoragePaths[Type]) ? "" : "/", pDir);
            return pBuffer;
        }

        void AddDefaultPaths()
        {
            AddPath("$USERDIR");
            AddPath("$DATADIR");
            AddPath("$CURRENTDIR");
        }

        void AddPath(string pPath)
        {
            if (m_NumPaths >= MAX_PATHS || string.IsNullOrEmpty(pPath))
                return;

            if (pPath == "$USERDIR")
            {
                if (!string.IsNullOrEmpty(m_aUserdir))
                {
                    m_aaStoragePaths[m_NumPaths++] = m_aUserdir;
                    CSystem.dbg_msg("storage", "added path '$USERDIR' ('{0}')", m_aUserdir);
                }
            }
            else if (pPath == "$DATADIR")
            {
                if (!string.IsNullOrEmpty(m_aDatadir))
                {
                    m_aaStoragePaths[m_NumPaths++] = m_aDatadir;
                    CSystem.dbg_msg("storage", "added path '$DATADIR' ('{0}')", m_aDatadir);
                }
            }
            else if (pPath == "$CURRENTDIR")
            {
                m_aaStoragePaths[m_NumPaths++] = m_aCurrentdir;
                CSystem.dbg_msg("storage", "added path '$CURRENTDIR' ('{0}')", m_aCurrentdir);
            }
            else
            {
                if (CSystem.fs_is_dir(CSystem.fs_getcwd() + "/" + pPath))
                {
                    m_aaStoragePaths[m_NumPaths++] = pPath;
                    CSystem.dbg_msg("storage", "added path '{0}'", pPath);
                }
            }
        }

        void FindDatadir(string pArgv0)
        {
            // 1) use data-dir in PWD if present
        }

        public override void ListDirectory(int Type, string pPath,
            Func<string, int, int, object, int> pfnCallback, object pUser)
        {
            if (Type == TYPE_ALL)
            {
                // list all available directories
                for (int i = 0; i < m_NumPaths; ++i)
                    CSystem.fs_listdir(GetPath(i, pPath), pfnCallback, i, pUser);
            }
            else if (Type >= 0 && Type < m_NumPaths)
            {
                // list wanted directory
                CSystem.fs_listdir(GetPath(Type, pPath), pfnCallback, Type, pUser);
            }
        }

        public override FileStream OpenFile(string pFilename, int Flags, int Type)
        {
            if ((Flags & CSystem.IOFLAG_WRITE) != 0)
            {
                return CSystem.io_open(GetPath(TYPE_SAVE, pFilename), Flags);
            }
        
            FileStream Handle;
            if (Type == TYPE_ALL)
            {
                // check all available directories
                for (int i = 0; i < m_NumPaths; ++i)
                {
                    Handle = CSystem.io_open(GetPath(i, pFilename), Flags);
                    if (Handle != null)
                        return Handle;
                }
            }
            else if (Type >= 0 && Type < m_NumPaths)
            {
                // check wanted directory
                Handle = CSystem.io_open(GetPath(Type, pFilename), Flags);
                if (Handle != null)
                    return Handle;
            }
            return null;
        }

        class CFindCBData
        {
            public CStorage pStorage;
            public string pFilename;
            public string pPath;
            public bool pFinded;
        }

        static int FindFileCallback(string pName, int IsDir, int Type, object pUser)
        {
            CFindCBData Data = (CFindCBData)pUser;

            if (IsDir != 0)
            {
                if (pName[0] == '.')
                    return 0;

                // search within the folder
                string aBuf;
                string aPath;

                aPath = string.Format("{0}/{1}", Data.pPath, pName);
                Data.pPath = aPath;

                CSystem.fs_listdir(Data.pStorage.GetPath(Type, aPath), FindFileCallback, Type, Data);
                if (Data.pFinded)
                    return 1;
            }
            else if (pName == Data.pFilename)
            {
                // found the file = end
                Data.pFinded = true;
                return 1;
            }

            return 0;
        }

        public override bool FindFile(string pFilename, string pPath, int Type, ref string pFilePath)
        {
            CFindCBData Data = new CFindCBData();
            Data.pStorage = this;
            Data.pFilename = pFilename;
            Data.pPath = pPath;
            Data.pFinded = false;

            if (Type == TYPE_ALL)
            {
                // search within all available directories
                for (int i = 0; i < m_NumPaths; ++i)
                {
                    CSystem.fs_listdir(GetPath(i, pPath), FindFileCallback, i, Data);
                    if (Data.pFinded)
                    {
                        pFilePath = string.Format("{0}/{1}", Data.pPath, Data.pFilename);
                        return true;
                    }
                }
            }
            else if (Type >= 0 && Type < m_NumPaths)
            {
                // search within wanted directory
                CSystem.fs_listdir(GetPath(Type, pPath), FindFileCallback, Type, Data);
                if (Data.pFinded)
                {
                    pFilePath = string.Format("{0}/{1}", Data.pPath, Data.pFilename);
                    return true;
                }
            }
            return Data.pFinded;
        }

        public override bool RemoveFile(string pFilename, int Type)
        {
            if (Type < 0 || Type >= m_NumPaths)
                return false;

            return CSystem.fs_remove(GetPath(Type, pFilename));
        }

        public override bool RenameFile(string pOldFilename, string pNewFilename, int Type)
        {
            if (Type < 0 || Type >= m_NumPaths)
                return false;
            return CSystem.fs_rename(GetPath(Type, pOldFilename), GetPath(Type, pNewFilename));
        }

        public override bool CreateFolder(string pFoldername, int Type)
        {
            if (Type < 0 || Type >= m_NumPaths)
                return false;

            return CSystem.fs_makedir(GetPath(Type, pFoldername));
        }

        public override string GetCompletePath(int Type, string pDir)
        {
            if (Type < 0 || Type >= m_NumPaths)
            {
                return "";
            }
            return GetPath(Type, pDir);
        }
    }
}
