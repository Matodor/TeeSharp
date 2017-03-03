using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public class CClient
    {
        public const int
            STATE_EMPTY = 0,
            STATE_AUTH = 1,
            STATE_CONNECTING = 2,
            STATE_READY = 3,
            STATE_INGAME = 4,

            SNAPRATE_INIT = 0,
            SNAPRATE_FULL = 1,
            SNAPRATE_RECOVER = 2,
            INPUT_COUNT = 200;

        public class CInput
        {
            public int[] m_aData = new int[(int)Consts.MAX_INPUT_SIZE];
            public int m_GameTick; // the tick that was chosen for the input
        }

        public int State { get { return m_State; } }
        public bool m_CustClt;
        public NETADDR m_Addr;

        // connection state info
        public int m_Latency;
        public int m_SnapRate;

        public float m_Traffic;
        public long m_TrafficSince;

        public int m_LastAckedSnapshot;
        public int m_LastInputTick;
        public CSnapshotStorage m_Snapshots;

        public CInput m_LatestInput;
        public CInput[] m_aInputs;
        public int m_CurrentInput;

        public string m_aName;
        public string m_aClan;
        public int m_Country;
        public int m_Score;
        public int m_Authed;
        public int m_AuthTries;

        public int m_Nonce;         // number to reach
        public int m_NonceCount;    // current num
        public long m_LastNonceCount;
        public IEnumerator<KeyValuePair<string, CConsoleCommand>> m_pRconCmdToSend;

        private int m_State;

        public CClient()
        {
            m_Addr = new NETADDR();
            m_Snapshots = new CSnapshotStorage();
            m_aInputs = new CInput[INPUT_COUNT];

            for (int i = 0; i < INPUT_COUNT; i++)
                m_aInputs[i] = new CInput();
        }

        public void SetState(int state)
        {
            m_State = state;
        }

        public void Reset()
        {
            // reset input
            for (int i = 0; i < INPUT_COUNT; i++)
                m_aInputs[i].m_GameTick = -1;

            m_CurrentInput = 0;
            m_LatestInput = new CInput();

            m_Snapshots.PurgeAll();
            m_LastAckedSnapshot = -1;
            m_LastInputTick = -1;
            m_SnapRate = SNAPRATE_INIT;
            m_Score = 0;
        }
    }
}
