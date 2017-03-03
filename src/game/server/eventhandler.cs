using System;
using System.Runtime.InteropServices;

namespace Teecsharp
{
    public class CEventHandler
    {
        public const int MAX_EVENTS = 128;

        private readonly int[] m_aTypes = new int[MAX_EVENTS];
        private readonly int[] m_aClientMasks = new int[MAX_EVENTS];
        private readonly CNetEvent_Common[] m_aData = new CNetEvent_Common[MAX_EVENTS];

        private CGameContext m_pGameServer;
        private int m_NumEvents;

        public CEventHandler()
        {
            m_pGameServer = null;
            Clear();
        }

        public void SetGameServer(CGameContext pGameServer)
        {
            m_pGameServer = pGameServer;
        }

        public T Create<T>(int Type, int Mask = -1) where T : CNetEvent_Common, new()
        {
            if (m_NumEvents == MAX_EVENTS)
                return null;

            m_aData[m_NumEvents] = new T();
            m_aTypes[m_NumEvents] = Type;
            m_aClientMasks[m_NumEvents] = Mask;

            return (T)m_aData[m_NumEvents++];
        }

        public void Clear()
        {
            m_NumEvents = 0;
        }

        public void Snap(int SnappingClient)
        {
            for (int i = 0; i < m_NumEvents; i++)
            {
                if (SnappingClient == -1 || CGameContext.CmaskIsSet(m_aClientMasks[i], SnappingClient))
                {
                    if (SnappingClient == -1 || VMath.distance(m_pGameServer.m_apPlayers[SnappingClient].m_ViewPos, m_aData[i].Pos()) < 1500.0f)
                    {
                        var d = (CNetEvent_Common)m_pGameServer.Server.SnapEvent(m_aData[i].GetType(), m_aTypes[i], i);
                        if (d != null)
                            m_aData[i].Write(d);
                    }
                }
            }
        }
    }
}
