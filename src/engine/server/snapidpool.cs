using Teecsharp;

class CSnapIDPool
{
    const int MAX_IDS = 32 * 1024;

    private class CID
    {
        public int m_Next;
        public int m_State; // 0 = free, 1 = alloced, 2 = timed
        public long m_Timeout;
    }

    private readonly CID[] m_aIDs;
    private int m_FirstFree;
    private int m_FirstTimed;
    private int m_LastTimed;

    public CSnapIDPool()
    {
        m_aIDs = new CID[MAX_IDS];
        for (int i = 0; i < MAX_IDS; i++)
            m_aIDs[i] = new CID();
        Reset();
    }

    public void Reset()
    {
        for (int i = 0; i < MAX_IDS; i++)
        {
            m_aIDs[i].m_Next = i + 1;
            m_aIDs[i].m_State = 0;
        }

        m_aIDs[MAX_IDS - 1].m_Next = -1;
        m_FirstFree = 0;
        m_FirstTimed = -1;
        m_LastTimed = -1;
    }

    public void RemoveFirstTimeout()
    {
        int NextTimed = m_aIDs[m_FirstTimed].m_Next;

        // add it to the free list
        m_aIDs[m_FirstTimed].m_Next = m_FirstFree;
        m_aIDs[m_FirstTimed].m_State = 0;
        m_FirstFree = m_FirstTimed;

        // remove it from the timed list
        m_FirstTimed = NextTimed;
        if (m_FirstTimed == -1)
            m_LastTimed = -1;
    }

    public int NewID()
    {
        var Now = CSystem.time_get();
        // process timed ids
        while (m_FirstTimed != -1 && m_aIDs[m_FirstTimed].m_Timeout < Now)
            RemoveFirstTimeout();

        int ID = m_FirstFree;
        if (ID == -1)
        {
            CSystem.dbg_msg("server", "id error");
            return ID;
        }

        m_FirstFree = m_aIDs[m_FirstFree].m_Next;
        m_aIDs[ID].m_State = 1;

        return ID;
    }

    public void TimeoutIDs()
    {
        // process timed ids
        while (m_FirstTimed != -1)
            RemoveFirstTimeout();
    }

    public void FreeID(int ID)
    {
        if (ID < 0)
            return;

        //dbg_assert(m_aIDs[ID].m_State == 1, "id is not alloced");

        m_aIDs[ID].m_State = 2;
        m_aIDs[ID].m_Timeout = CSystem.time_get() + CSystem.time_freq() * 5;
        m_aIDs[ID].m_Next = -1;

        if (m_LastTimed != -1)
        {
            m_aIDs[m_LastTimed].m_Next = ID;
            m_LastTimed = ID;
        }
        else
        {
            m_FirstTimed = ID;
            m_LastTimed = ID;
        }
    }
}