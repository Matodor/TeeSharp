using System;

namespace Teecsharp
{
    public abstract class IConsole : IInterface
    {
        /*//	TODO: rework/cleanup
        public const int 
            OUTPUT_LEVEL_STANDARD = 0,
            OUTPUT_LEVEL_ADDINFO = 1,
            OUTPUT_LEVEL_DEBUG = 2,

            ACCESS_LEVEL_ADMIN = 0,
            ACCESS_LEVEL_MOD = 1,

            TEMPCMD_NAME_LENGTH = 32,
            TEMPCMD_HELP_LENGTH = 96,
            TEMPCMD_PARAMS_LENGTH = 16,

            MAX_PRINT_CB = 4;

        // TODO: rework this interface to reduce the amount of virtual calls
        public abstract class IResult
        {
            protected uint m_NumArgs;

            public IResult()
            {
                m_NumArgs = 0;
            }

            ~IResult()
            {
                
            }

            public abstract int GetInteger(uint Index);
            public abstract float GetFloat(uint Index);
		    public abstract string GetString(uint Index);

            public uint NumArguments()
            {
                return m_NumArgs;
            }
        }

        public abstract class CCommandInfo
        {
            protected int m_AccessLevel;

            public CCommandInfo()
            {
                m_AccessLevel = ACCESS_LEVEL_ADMIN;
            }

            ~CCommandInfo()
            {
                
            }

            public string m_pName;
            public string m_pHelp;
            public string m_pParams;

            public abstract CCommandInfo NextCommandInfo(int AccessLevel, int FlagMask);

            public int GetAccessLevel()
            {
                return m_AccessLevel;
            }
        }

        public delegate void FPrintCallback(string pStr, object pUser);
        public delegate void FPossibleCallback(string pCmd, object pUser);
        public delegate void FCommandCallback(IResult pResult, object pUserData);
        public delegate void FChainCommandCallback(IResult pResult, object pUserData, 
            FCommandCallback pfnCallback, object pCallbackUserData);

        public abstract CCommandInfo FirstCommandInfo(int AccessLevel, int Flagmask);
        public abstract CCommandInfo GetCommandInfo(string pName, int FlagMask, bool Temp);
        public abstract void PossibleCommands(string pStr, int FlagMask, bool Temp, 
            FPossibleCallback pfnCallback, object pUser);
        public abstract void ParseArguments(params string[] ppArguments);

        public abstract void Register(string pName, string pParams, int Flags, FCommandCallback pfnFunc,
            object pUser, string pHelp);
        public abstract void RegisterTemp(string pName, string pParams, int Flags, string pHelp);
        public abstract void DeregisterTemp(string pName);
        public abstract void DeregisterTempAll();
	    public abstract void Chain(string pName, FChainCommandCallback pfnChainFunc, object pUser);
        public abstract void StoreCommands(bool Store);

        public abstract bool LineIsValid(string pStr);
        public abstract void ExecuteLine(string pStr);
        public abstract void ExecuteLineFlag(string pStr, int FlagMask);
        public abstract void ExecuteFile(string pFilename);

        public abstract int RegisterPrintCallback(int OutputLevel, FPrintCallback pfnPrintCallback, 
            object pUserData);
	    public abstract void SetPrintOutputLevel(int Index, int OutputLevel);
	    public abstract void Print(int Level, string pFrom, string pStr, ConsoleColor color = ConsoleColor.White);

        public abstract void SetAccessLevel(int AccessLevel);*/

        public const int
            OUTPUT_LEVEL_STANDARD = 0,
            OUTPUT_LEVEL_ADDINFO = 1,
            OUTPUT_LEVEL_DEBUG = 2,

            ACCESS_LEVEL_ADMIN = 0,
            ACCESS_LEVEL_MOD = 1,
            ACCESS_LEVEL_NO = 2,

            TEMPCMD_NAME_LENGTH = 32,
            TEMPCMD_HELP_LENGTH = 96,
            TEMPCMD_PARAMS_LENGTH = 16,

            MAX_PRINT_CB = 4;

        public abstract void Print(int level, string from, string str);
        public abstract void ExecuteFile(string fileName);
        public abstract void ExecuteLine(string line, int accessLevel);
        public abstract void ParseArguments(string[] args);

        public abstract void Register(string cmd, string format, int flags, FConsoleCallback callback, object data, string help, int accessLevel = ACCESS_LEVEL_ADMIN);
        public abstract int RegisterPrintCallback(int outputLevel, FPrintCallback callback, object data);
        public abstract void Chain(string name, FChainCommandCallback chainFunc, object user);
        public abstract void Init();
    }
}
