using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public class CEngine : IEngine
    {
        public IConsole m_pConsole;
        public IStorage m_pStorage;

        private bool m_Logging;

        private const int
            CFGFLAG_SERVER = CConfiguration.CFGFLAG_SERVER,
            CFGFLAG_CLIENT = CConfiguration.CFGFLAG_CLIENT;


        public CEngine(string pAppname)
        {
            #if DEBUG
                CSystem.dbg_msg_clr("server", "running on debug version", ConsoleColor.Red);
            #endif
            m_JobPool = new CJobPool();
            m_JobPool.Init(1);
            m_Logging = false;
        }

        public override void Init()
        {
            m_pConsole = Kernel.RequestInterface<IConsole>();
            m_pStorage = Kernel.RequestInterface<IStorage>();

            if (m_pConsole == null || m_pStorage == null)
                return;

            //m_pConsole.Register("dbg_dumpmem", "", CFGFLAG_SERVER | CFGFLAG_CLIENT, Con_DbgDumpmem, this, "Dump the memory");
            //m_pConsole.Register("dbg_lognetwork", "", CFGFLAG_SERVER | CFGFLAG_CLIENT, Con_DbgLognetwork, this, "Log the network");
        }

        public override void InitLogfile()
        {
            
        }

        private static bool HostLookupThread(object pUser)
        {
            CHostLookup pLookup = (CHostLookup)pUser;
            return CSystem.net_host_lookup(pLookup.m_aHostname, ref pLookup.m_Addr, pLookup.m_Nettype);
        }

        public override void HostLookup(CHostLookup pLookup, string pHostname, int Nettype)
        {
            pLookup.m_aHostname = pHostname;
            pLookup.m_Nettype = Nettype;
            AddJob(pLookup.m_Job, HostLookupThread, pLookup);
        }

        public override void AddJob(CJob pJob, JOBFUNC pfnFunc, object pData)
        {
            if (g_Config.GetInt("Debug") != 0)
                CSystem.dbg_msg("engine", "job added");
            m_JobPool.Add(pJob, pfnFunc, pData);
        }

        public static IEngine CreateEngine(string pAppname)
        {
            return new CEngine(pAppname);
        }
    }
}
