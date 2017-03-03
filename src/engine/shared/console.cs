using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public partial class CConsole
    {
        /*
        private class CCommand : CCommandInfo
        {
            public CCommand m_pNext;
            public int m_Flags;
            public bool m_Temp;
            public FCommandCallback m_pfnCallback;
            public object m_pUserData;

            public void SetAccessLevel(int AccessLevel)
            {
                m_AccessLevel = CMath.clamp(AccessLevel, ACCESS_LEVEL_ADMIN, ACCESS_LEVEL_MOD);
            }

            public override CCommandInfo NextCommandInfo(int AccessLevel, int FlagMask)
            {
                CCommand pInfo = m_pNext;
                while (pInfo != null)
                {
                    if ((pInfo.m_Flags & FlagMask) != 0 && pInfo.m_AccessLevel >= AccessLevel)
                        break;
                    pInfo = pInfo.m_pNext;
                }
                return pInfo;
            }
        }

        private class CChain
        {
            public FChainCommandCallback m_pfnChainCallback;
            public FCommandCallback m_pfnCallback;
            public object m_pCallbackUserData;
            public object m_pUserData;
        }

        private int m_FlagMask;
        private bool m_StoreCommands;
        private string[] m_paStrokeStr = new string[2];
        private CCommand m_pFirstCommand;

        private class CExecFile
        {
            public string m_pFilename;
            public CExecFile m_pPrev;
        }

        private CExecFile m_pFirstExec;
        private IStorage m_pStorage;
        private int m_AccessLevel;

        private CCommand m_pRecycleList;
        private Queue<CCommand> m_TempCommands = new Queue<CCommand>();

        private CCommand FindCommand(string pName, int FlagMask)
        {
            for (CCommand pCommand = m_pFirstCommand; pCommand != null; pCommand = pCommand.m_pNext)
            {
                if ((pCommand.m_Flags & FlagMask) != 0)
                {
                    if (CSystem.str_comp_nocase(pCommand.m_pName, pName) == 0)
                        return pCommand;
                }
            }
            return null;
        }

        public static void Con_Chain(IResult pResult, object pUserData)
        {
            CChain pInfo = (CChain) pUserData;
            pInfo.m_pfnChainCallback(pResult, pInfo.m_pUserData, pInfo.m_pfnCallback, pInfo.m_pCallbackUserData);
        }

        public static void Con_Echo(IResult pResult, object pUserData)
        {
        }

        public static void Con_Exec(IResult pResult, object pUserData)
        {
        }

        public static void ConToggle(IResult pResult, object pUser)
        {
        }

        public static void ConToggleStroke(IResult pResult, object pUser)
        {
        }

        public static void ConModCommandAccess(IResult pResult, object pUser)
        {
            CConsole pConsole = (CConsole) pUser;
            string aBuf = "";
            CCommand pCommand = pConsole.FindCommand(pResult.GetString(0), CConfiguration.CFGFLAG_SERVER);
            if (pCommand != null)
            {
                if (pResult.NumArguments() == 2)
                {
                    pCommand.SetAccessLevel(pResult.GetInteger(1));
                    aBuf = string.Format("moderator access for '{0}' is now {1}",
                        pResult.GetString(0), pCommand.GetAccessLevel() != 0 ? "enabled" : "disabled");
                }
                else
                    aBuf = string.Format("moderator access for '{0}' is {1}", pResult.GetString(0),
                        pCommand.GetAccessLevel() != 0 ? "enabled" : "disabled");
            }
            else
                aBuf = string.Format("No such command: '{0}'.", pResult.GetString(0));

            pConsole.Print(OUTPUT_LEVEL_STANDARD, "Console", aBuf);
        }

        public static void ConModCommandStatus(IResult pResult, object pUser)
        {

        }

        public void ExecuteFileRecurse(string pFilename)
        {

        }

        struct PrintCB
        {
            public int m_OutputLevel;
            public FPrintCallback m_pfnPrintCallback;
            public object m_pPrintCallbackUserdata;
        }

        PrintCB[] m_aPrintCB = new PrintCB[MAX_PRINT_CB];
        int m_NumPrintCB;

        const int
            CONSOLE_MAX_STR_LENGTH = 1024,
            MAX_PARTS = (CONSOLE_MAX_STR_LENGTH + 1)/2;

        public class CResult : IResult
        {
            public string m_aStringStorage;
            public string m_pCommand;
            public string m_pArgs;
            public string[] m_apArgs = new string[MAX_PARTS];

            public CResult()
            {
                m_aStringStorage = "";
                m_pArgs = "";
                m_pCommand = "";
            }

            public void AddArgument(string pArg)
            {
                m_apArgs[m_NumArgs++] = pArg;
            }

            public override int GetInteger(uint Index)
            {
                if (Index >= m_NumArgs)
                    return 0;
                return int.Parse(m_apArgs[Index]);
            }

            public override float GetFloat(uint Index)
            {
                if (Index >= m_NumArgs)
                    return 0.0f;
                return float.Parse(m_apArgs[Index]);
            }

            public override string GetString(uint Index)
            {
                if (Index >= m_NumArgs)
                    return "";
                return m_apArgs[Index];
            }
        }

        class CExecutionQueue
        {
            private Queue<CQueueEntry> m_Queue = new Queue<CQueueEntry>();

            public class CQueueEntry
            {
                public CQueueEntry m_pNext;
                public FCommandCallback m_pfnCommandCallback;
                public object m_pCommandUserData;
                public CResult m_Result = new CResult();
            }

            public CQueueEntry m_pFirst, m_pLast;

            public void AddEntry()
            {
                CQueueEntry pEntry = new CQueueEntry();
                pEntry.m_pNext = null;
                if (m_pFirst == null)
                    m_pFirst = pEntry;
                if (m_pLast != null)
                    m_pLast.m_pNext = pEntry;
                m_pLast = pEntry;
                m_Queue.Enqueue(pEntry);
            }

            public void Reset()
            {
                m_Queue.Clear();
                m_pFirst = m_pLast = null;
            }
        }

        private CExecutionQueue m_ExecutionQueue = new CExecutionQueue();

        public CConsole(int FlagMask)
        {
            m_FlagMask = FlagMask;
            m_AccessLevel = ACCESS_LEVEL_ADMIN;
            m_pRecycleList = null;
            m_TempCommands.Clear();
            m_StoreCommands = true;
            m_paStrokeStr[0] = "0";
            m_paStrokeStr[1] = "1";
            m_ExecutionQueue.Reset();
            m_pFirstCommand = null;
            m_pFirstExec = null;
            for (int i = 0; i < m_aPrintCB.Length; i++)
                m_aPrintCB[i] = new PrintCB();
            m_NumPrintCB = 0;

            m_pStorage = null;

            // register some basic commands
            Register("echo", "r", CConfiguration.CFGFLAG_SERVER | CConfiguration.CFGFLAG_CLIENT, Con_Echo, this,
                "Echo the text");
            Register("exec", "r", CConfiguration.CFGFLAG_SERVER | CConfiguration.CFGFLAG_CLIENT, Con_Exec, this,
                "Execute the specified file");

            Register("mod_command", "s?i", CConfiguration.CFGFLAG_SERVER, ConModCommandAccess, this,
                "Specify command accessibility for moderators");
            Register("mod_status", "", CConfiguration.CFGFLAG_SERVER, ConModCommandStatus, this,
                "List all commands which are accessible for moderators");

            foreach (var variable in CConfiguration.Variables)
            {
                if (variable.Value.GetType() == typeof (ConfigInt))
                {
                    ConfigInt Data = (ConfigInt) variable.Value;
                    Register(Data.ScriptName, "?i", Data.Flags, IntVariableCommand, Data, Data.Desc);
                }
                else if (variable.Value.GetType() == typeof (ConfigStr))
                {
                    ConfigStr Data = (ConfigStr) variable.Value;
                    Register(Data.ScriptName, "?r", Data.Flags, StrVariableCommand, Data, Data.Desc);
                }
            }
        }

        void StrVariableCommand(IResult pResult, object pUserData)
        {
            ConfigStr pData = (ConfigStr) pUserData;

            if (pResult.NumArguments() != 0)
            {
                string pString = pResult.GetString(0);

                //if (!str_utf8_check(pString))
                //{
                // char Temp[4];
                // int Length = 0;
                // while (*pString)
                // {
                //    int Size = str_utf8_encode(Temp, static_cast <const unsigned char> (*pString++));
                //    if (Length + Size < pData.m_MaxSize)
                //   {
                //      mem_copy(pData.m_pStr + Length, &Temp, Size);
                //       Length += Size;
                //    }
                //     else
                //          break;
                //   }
                //   pData.Default[Length] = 0;
                //}
                //else
                pData.Default = pString;
            }
            else
            {
                string aBuf = string.Format("Value: %s", pData.Default);
                Print(OUTPUT_LEVEL_STANDARD, "Console", aBuf);
            }
        }

        void IntVariableCommand(IResult pResult, object pUserData)
        {
            ConfigInt pData = (ConfigInt) pUserData;

            if (pResult.NumArguments() != 0)
            {
                int Val = pResult.GetInteger(0);

                // do clamping
                if (pData.Min != pData.Max)
                {
                    if (Val < pData.Min)
                        Val = pData.Min;
                    if (pData.Max != 0 && Val > pData.Max)
                        Val = pData.Max;
                }

                pData.Default = Val;
            }
            else
            {
                string aBuf = string.Format("Value: {0}", pData.Default);
                Print(OUTPUT_LEVEL_STANDARD, "Console", aBuf);
            }
        }

        public void ParseStart(CResult pResult, string pString)
        {
            pResult.m_aStringStorage = pString;

            var clearStr = CSystem.str_skip_whitespaces(pString);
            var spacePos = clearStr.IndexOf(' ');
            if (spacePos > 0)
            {
                pResult.m_pCommand = clearStr.Substring(0, spacePos);
                if (spacePos > 0 && spacePos + 1 < clearStr.Length)
                    pResult.m_pArgs = clearStr.Substring(spacePos + 1, clearStr.Length - spacePos - 1);
            }
            else
            {
                pResult.m_pCommand = clearStr;
            }
        }

        public bool ParseArgs(CResult pResult, string pFormat)
        {
            var Error = false;
            var Optional = false;
            var pFormatArgs = pFormat.ToCharArray();
            int i = 0, j = 0;
            var args = CSystem.str_skip_whitespaces(pResult.m_pArgs).Split(' ');

            while (i < args.Length)
            {
                // fetch command
                char? Command = null;
                if (j < pFormatArgs.Length)
                {
                    Command = pFormatArgs[j];
                    j++;
                }

                if (Command == null)
                    break;

                if (Command == '?')
                    Optional = true;
                else
                {
                    var pStr = args[i];
                    if (!Optional && string.IsNullOrEmpty(pStr))
                    {
                        // error, non optional command needs value
                        Error = true;
                        break;
                    }

                    // add token
                    //if (pStr[0] == '"')
                    //{
                    //
                    //}
                    //else
                    //{
                    pResult.AddArgument(pStr);
                    //}
                    i++;
                }
            }

            return Error;
        }

        public override CCommandInfo FirstCommandInfo(int AccessLevel, int Flagmask)
        {
            for (CCommand pCommand = m_pFirstCommand; pCommand != null; pCommand = pCommand.m_pNext)
            {
                if ((pCommand.m_Flags & Flagmask) != 0 && pCommand.GetAccessLevel() >= AccessLevel)
                    return pCommand;
            }

            return null;
        }

        public override CCommandInfo GetCommandInfo(string pName, int FlagMask, bool Temp)
        {
            for (CCommand pCommand = m_pFirstCommand; pCommand != null; pCommand = pCommand.m_pNext)
            {
                if ((pCommand.m_Flags & FlagMask) != 0 && pCommand.m_Temp == Temp)
                {
                    if (CSystem.str_comp_nocase(pCommand.m_pName, pName) == 0)
                        return pCommand;
                }
            }

            return null;
        }

        public override void PossibleCommands(string pStr, int FlagMask, bool Temp, FPossibleCallback pfnCallback,
            object pUser)
        {
            throw new NotImplementedException();
        }

        public override void ParseArguments(params string[] ppArguments)
        {
            for (int i = 0; i < ppArguments.Length; i++)
            {
                // check for scripts to execute
                if (ppArguments[i][0] == '-' && ppArguments[i][1] == 'f')
                {
                    if (ppArguments.Length - i > 1)
                        ExecuteFile(ppArguments[i + 1]);
                    i++;
                }
                else if (CSystem.str_comp("-s", ppArguments[i]) == 0 || CSystem.str_comp("--silent", ppArguments[i]) == 0)
                {
                    // skip silent param
                    continue;
                }
                else
                {
                    // search arguments for overrides
                    ExecuteLine(ppArguments[i]);
                }
            }
        }

        void AddCommandSorted(CCommand pCommand)
        {
            if (m_pFirstCommand == null || CSystem.str_comp(pCommand.m_pName, m_pFirstCommand.m_pName) < 0)
            {
                if (m_pFirstCommand?.m_pNext != null)
                    pCommand.m_pNext = m_pFirstCommand;
                else
                    pCommand.m_pNext = null;
                m_pFirstCommand = pCommand;
            }
            else
            {
                for (CCommand p = m_pFirstCommand; p != null; p = p.m_pNext)
                {
                    if (p.m_pNext == null || CSystem.str_comp(pCommand.m_pName, p.m_pNext.m_pName) < 0)
                    {
                        pCommand.m_pNext = p.m_pNext;
                        p.m_pNext = pCommand;
                        break;
                    }
                }
            }
        }

        public sealed override void Register(string pName, string pParams, int Flags, FCommandCallback pfnFunc,
            object pUser, string pHelp)
        {
            CCommand pCommand = new CCommand();
            pCommand.m_pfnCallback = pfnFunc;
            pCommand.m_pUserData = pUser;

            pCommand.m_pName = pName;
            pCommand.m_pHelp = pHelp;
            pCommand.m_pParams = pParams;

            pCommand.m_Flags = Flags;
            pCommand.m_Temp = false;

            AddCommandSorted(pCommand);
        }

        public override void RegisterTemp(string pName, string pParams, int Flags, string pHelp)
        {
            CCommand pCommand;

            if (m_pRecycleList != null)
            {
                pCommand = m_pRecycleList;
                pCommand.m_pName = pName;
                pCommand.m_pHelp = pHelp;
                pCommand.m_pParams = pParams;
                m_pRecycleList = m_pRecycleList.m_pNext;
            }
            else
            {
                pCommand = new CCommand();
                pCommand.m_pName = pName;
                pCommand.m_pHelp = pHelp;
                pCommand.m_pParams = pParams;

                m_TempCommands.Enqueue(pCommand);
            }

            pCommand.m_pfnCallback = null;
            pCommand.m_pUserData = null;
            pCommand.m_Flags = Flags;
            pCommand.m_Temp = true;

            AddCommandSorted(pCommand);
        }

        public override void DeregisterTemp(string pName)
        {
            if (m_pFirstCommand == null)
                return;

            CCommand pRemoved = null;

            // remove temp entry from command list
            if (m_pFirstCommand.m_Temp && m_pFirstCommand.m_pName == pName)
            {
                pRemoved = m_pFirstCommand;
                m_pFirstCommand = m_pFirstCommand.m_pNext;
            }
            else
            {
                for (CCommand pCommand = m_pFirstCommand; pCommand.m_pNext != null; pCommand = pCommand.m_pNext.m_pNext)
                    if (pCommand.m_pNext.m_Temp && pCommand.m_pNext.m_pName == pName)
                    {
                        pRemoved = pCommand.m_pNext;
                        pCommand.m_pNext = pCommand.m_pNext.m_pNext;
                        break;
                    }
            }

            // add to recycle list
            if (pRemoved != null)
            {
                pRemoved.m_pNext = m_pRecycleList;
                m_pRecycleList = pRemoved;
            }
        }

        public override void DeregisterTempAll()
        {
            // set non temp as first one
            for (; m_pFirstCommand != null && m_pFirstCommand.m_Temp; m_pFirstCommand = m_pFirstCommand.m_pNext) ;

            // remove temp entries from command list
            for (CCommand pCommand = m_pFirstCommand;
                pCommand != null && pCommand.m_pNext != null;
                pCommand = pCommand.m_pNext)
            {
                CCommand pNext = pCommand.m_pNext;
                if (pNext.m_Temp)
                {
                    for (; pNext != null && pNext.m_Temp; pNext = pNext.m_pNext) ;
                    pCommand.m_pNext = pNext;
                }
            }

            m_TempCommands.Clear();
            m_pRecycleList = null;
        }

        public override void Chain(string pName, FChainCommandCallback pfnChainFunc, object pUser)
        {
            CCommand pCommand = FindCommand(pName, m_FlagMask);

            if (pCommand == null)
            {
                string aBuf = string.Format("failed to chain '{0}'", pName);
                Print(OUTPUT_LEVEL_DEBUG, "console", aBuf);
                return;
            }

            CChain pChainInfo = new CChain();

            // store info
            pChainInfo.m_pfnChainCallback = pfnChainFunc;
            pChainInfo.m_pUserData = pUser;
            pChainInfo.m_pfnCallback = pCommand.m_pfnCallback;
            pChainInfo.m_pCallbackUserData = pCommand.m_pUserData;

            // chain
            pCommand.m_pfnCallback = Con_Chain;
            pCommand.m_pUserData = pChainInfo;
        }

        public override void StoreCommands(bool Store)
        {
            if (!Store)
            {
                for (CExecutionQueue.CQueueEntry pEntry = m_ExecutionQueue.m_pFirst;
                    pEntry != null;
                    pEntry = pEntry.m_pNext)
                    pEntry.m_pfnCommandCallback(pEntry.m_Result, pEntry.m_pCommandUserData);
                m_ExecutionQueue.Reset();
            }
            m_StoreCommands = Store;
        }

        public override bool LineIsValid(string pStr)
        {
            throw new NotImplementedException();
        }

        public override void ExecuteLineFlag(string pStr, int FlagMask)
        {
            int Temp = m_FlagMask;
            m_FlagMask = FlagMask;
            ExecuteLine(pStr);
            m_FlagMask = Temp;
        }

        public override void ExecuteLine(string pStr)
        {
            CResult Result = new CResult();
            ParseStart(Result, pStr);

            if (string.IsNullOrEmpty(Result.m_pCommand))
                return;

            CCommand pCommand = FindCommand(Result.m_pCommand, m_FlagMask);
            if (pCommand != null)
            {
                if (pCommand.GetAccessLevel() >= m_AccessLevel)
                {
                    if (ParseArgs(Result, pCommand.m_pParams))
                    {
                        string aBuf = string.Format("Invalid arguments... Usage: {0} {1}", pCommand.m_pName,
                            pCommand.m_pParams);
                        Print(OUTPUT_LEVEL_STANDARD, "Console", aBuf);
                    }
                    else if (m_StoreCommands && (pCommand.m_Flags & CServer.CFGFLAG_STORE) != 0)
                    {
                        m_ExecutionQueue.AddEntry();
                        m_ExecutionQueue.m_pLast.m_pfnCommandCallback = pCommand.m_pfnCallback;
                        m_ExecutionQueue.m_pLast.m_pCommandUserData = pCommand.m_pUserData;
                        m_ExecutionQueue.m_pLast.m_Result = Result;
                    }
                    else
                        pCommand.m_pfnCallback(Result, pCommand.m_pUserData);
                }
                else
                {
                    string aBuf = string.Format("Access for command {0} denied.", Result.m_pCommand);
                    Print(OUTPUT_LEVEL_STANDARD, "Console", aBuf);
                }
            }
            else
            {
                string aBuf = string.Format("No such command: {0}.", Result.m_pCommand);
                Print(OUTPUT_LEVEL_STANDARD, "Console", aBuf);
            }
        }

        public override void ExecuteFile(string pFilename)
        {
            // make sure that this isn't being executed already
            for (CExecFile pCur = m_pFirstExec; pCur != null; pCur = pCur.m_pPrev)
                if (pFilename == pCur.m_pFilename)
                    return;

            if (m_pStorage == null)
                m_pStorage = Kernel.RequestInterface<IStorage>();
            if (m_pStorage == null)
                return;

            // push this one to the stack
            CExecFile ThisFile = new CExecFile();
            CExecFile pPrev = m_pFirstExec;

            ThisFile.m_pFilename = pFilename;
            ThisFile.m_pPrev = m_pFirstExec;
            m_pFirstExec = ThisFile;

            // exec the file
            var File = m_pStorage.OpenFile(pFilename, CSystem.IOFLAG_READ, IStorage.TYPE_ALL);

            string aBuf;
            if (File != null)
            {
                string pLine;
                TextReader LineReader = new StreamReader(File);

                aBuf = string.Format("executing '{0}'", pFilename);
                Print(OUTPUT_LEVEL_STANDARD, "console", aBuf);

                while ((pLine = LineReader.ReadLine()) != null)
                    ExecuteLine(pLine);
                CSystem.io_close(File);
            }
            else
            {
                aBuf = string.Format("failed to open '{0}'", pFilename);
                Print(OUTPUT_LEVEL_STANDARD, "console", aBuf);
            }
        }

        public override int RegisterPrintCallback(int OutputLevel, FPrintCallback pfnPrintCallback, object pUserData)
        {
            if (m_NumPrintCB == MAX_PRINT_CB)
                return -1;

            m_aPrintCB[m_NumPrintCB].m_OutputLevel = CMath.clamp(OutputLevel, OUTPUT_LEVEL_STANDARD, OUTPUT_LEVEL_DEBUG);
            m_aPrintCB[m_NumPrintCB].m_pfnPrintCallback = pfnPrintCallback;
            m_aPrintCB[m_NumPrintCB].m_pPrintCallbackUserdata = pUserData;
            return m_NumPrintCB++;
        }

        public override void SetPrintOutputLevel(int Index, int OutputLevel)
        {
            if (Index >= 0 && Index < MAX_PRINT_CB)
                m_aPrintCB[Index].m_OutputLevel = CMath.clamp(OutputLevel, OUTPUT_LEVEL_STANDARD, OUTPUT_LEVEL_DEBUG);
        }

        public override void Print(int Level, string pFrom, string pStr, ConsoleColor color = ConsoleColor.White)
        {
            CSystem.dbg_msg_clr(pFrom, "{0}", color, pStr);
            for (int i = 0; i < m_NumPrintCB; ++i)
            {
                if (Level <= m_aPrintCB[i].m_OutputLevel && m_aPrintCB[i].m_pfnPrintCallback != null)
                {
                    string aBuf = string.Format("[{0}]: {1}", pFrom, pStr);
                    m_aPrintCB[i].m_pfnPrintCallback(aBuf, m_aPrintCB[i].m_pPrintCallbackUserdata);
                }
            }
        }

        public override void SetAccessLevel(int AccessLevel)
        {
            m_AccessLevel = CMath.clamp(AccessLevel, ACCESS_LEVEL_ADMIN, ACCESS_LEVEL_MOD);
        }
        */
    }
}
