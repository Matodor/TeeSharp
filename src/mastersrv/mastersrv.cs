using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public partial class CMasterServer : IMasterServer
    {
        public const int
            STATE_INIT = 0,
            STATE_UPDATE = 1,
            STATE_READY = 2;

        // master server functions
        public class CMasterInfo
        {
            public string m_aHostname;
            public NETADDR m_Addr;
            public bool m_Valid;
            public CHostLookup m_Lookup = new CHostLookup();
        }

        public CMasterInfo[] m_aMasterServers;
        public int m_State;
        public IEngine m_pEngine;
        public IStorage m_pStorage;

        public CMasterServer()
        {
            m_aMasterServers = new CMasterInfo[MAX_MASTERSERVERS];
            m_State = STATE_INIT;
            m_pEngine = null;
            m_pStorage = null;
            SetDefault();
        }

        public override void Init()
        {
            m_pEngine = Kernel.RequestInterface<IEngine>();
            m_pStorage = Kernel.RequestInterface<IStorage>();
        }

        public sealed override void SetDefault()
        {
            for (int i = 0; i < MAX_MASTERSERVERS; i++)
            {
                m_aMasterServers[i] = new CMasterInfo();
                m_aMasterServers[i].m_aHostname = string.Format("master{0}.teeworlds.com", i + 1);
            }
        }

        public static IMasterServer CreateEngineMasterServer()
        {
            return new CMasterServer();
        }

        public override int Load()
        {
            return 0;
        }

        public override int Save()
        {
            return 0;
        }

        public override int RefreshAddresses(int Nettype)
        {
            if (m_State != STATE_INIT && m_State != STATE_READY)
                return -1;

            CSystem.dbg_msg("engine/mastersrv", "refreshing master server addresses");

            // add lookup jobs
            for (int i = 0; i < MAX_MASTERSERVERS; i++)
            {
                m_pEngine.HostLookup(m_aMasterServers[i].m_Lookup, m_aMasterServers[i].m_aHostname, Nettype);
                m_aMasterServers[i].m_Valid = false;
            }

            m_State = STATE_UPDATE;
            return 0;
        }

        public override void Update()
        {
            // check if we need to update
            if (m_State != STATE_UPDATE)
                return;
            m_State = STATE_READY;

            for (int i = 0; i < MAX_MASTERSERVERS; i++)
            {
                if (m_aMasterServers[i].m_Lookup.m_Job.Status() != CJob.STATE_DONE)
                    m_State = STATE_UPDATE;
                else
                {
                    if (m_aMasterServers[i].m_Lookup.m_Job.Result())
                    {
                        m_aMasterServers[i].m_Addr = m_aMasterServers[i].m_Lookup.m_Addr;
                        m_aMasterServers[i].m_Addr.port = 8300;
                        m_aMasterServers[i].m_Valid = true;
                    }
                    else
                        m_aMasterServers[i].m_Valid = false;
                }
            }

            if (m_State == STATE_READY)
            {
                CSystem.dbg_msg("engine/mastersrv", "saving addresses");
                Save();
            }
        }

        public override int IsRefreshing()
        {
            return m_State != STATE_READY ? 1 : 0;
        }

        public override NETADDR GetAddr(int Index)
        {
            return m_aMasterServers[Index].m_Addr;
        }

        public override string GetName(int Index)
        {
            return m_aMasterServers[Index].m_aHostname;
        }

        public override bool IsValid(int Index)
        {
            return m_aMasterServers[Index].m_Valid;
        }
    }

    public abstract class IMasterServer : IInterface
    {
        public const int MAX_MASTERSERVERS = 4;

        public abstract void Init();
        public abstract void SetDefault();
        public abstract int Load();
        public abstract int Save();

        public abstract int RefreshAddresses(int Nettype);
        public abstract void Update();
        public abstract int IsRefreshing();
        public abstract NETADDR GetAddr(int Index);
        public abstract string GetName(int Index);
        public abstract bool IsValid(int Index);
    }
    
    public partial class CMasterServer
    {
        public static readonly byte[] SERVERBROWSE_HEARTBEAT = { 255, 255, 255, 255, 98, 101, 97, 50 };

        public static readonly byte[] SERVERBROWSE_GETLIST  = { 255, 255, 255, 255, 114, 101, 113, 50 };
        public static readonly byte[] SERVERBROWSE_LIST = { 255, 255, 255, 255, 108, 105, 115, 50 };

        public static readonly byte[] SERVERBROWSE_GETCOUNT = { 255, 255, 255, 255, 99, 111, 117, 50 };
        public static readonly byte[] SERVERBROWSE_COUNT = { 255, 255, 255, 255, 115, 105, 122, 50 };

        public static readonly byte[] SERVERBROWSE_GETINFO = { 255, 255, 255, 255, 103, 105, 101, 51 };
        public static readonly byte[] SERVERBROWSE_INFO = { 255, 255, 255, 255, 105, 110, 102, 51 };

        public static readonly byte[] SERVERBROWSE_GETINFO64 = { 255, 255, 255, 255, 102, 115, 116, 100 };
        public static readonly byte[] SERVERBROWSE_INFO64 = { 255, 255, 255, 255, 100, 116, 115, 102 };

        public static readonly byte[] SERVERBROWSE_FWCHECK = { 255, 255, 255, 255, 102, 119, 63, 63 };
        public static readonly byte[] SERVERBROWSE_FWRESPONSE = { 255, 255, 255, 255, 102, 119,33, 33 };
        public static readonly byte[] SERVERBROWSE_FWOK = { 255, 255, 255, 255, 102, 119, 111, 107 };
        public static readonly byte[] SERVERBROWSE_FWERROR = { 255, 255, 255, 255, 102, 119, 101, 114 };

        public static readonly byte[] SERVERBROWSE_HEARTBEAT_LEGACY = { 255, 255, 255, 255, 98, 101, 97, 116 };

        public static readonly byte[] SERVERBROWSE_GETLIST_LEGACY = { 255, 255, 255, 255, 114, 101, 113, 116 };
        public static readonly byte[] SERVERBROWSE_LIST_LEGACY = { 255, 255, 255, 255, 108, 105, 115, 116 };

        public static readonly byte[] SERVERBROWSE_GETCOUNT_LEGACY = { 255, 255, 255, 255, 99, 111, 117, 110 };
        public static readonly byte[] SERVERBROWSE_COUNT_LEGACY = { 255, 255, 255, 255, 115, 105, 122, 101 };
    }
}
