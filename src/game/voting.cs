namespace Teecsharp
{
    public static class CVoteConsts
    {
        public const int
            VOTE_DESC_LENGTH = 64,
            VOTE_CMD_LENGTH = 512,
            VOTE_REASON_LENGTH = 16,
            MAX_VOTE_OPTIONS = 128;
    }

    public class CVoteOptionClient
    {
        public CVoteOptionClient m_pNext;
        public CVoteOptionClient m_pPrev;
        public string m_aDescription;
    }

    public class CVoteOptionServer
    {
        public CVoteOptionServer m_pNext;
        public CVoteOptionServer m_pPrev;
        public string m_aDescription;
        public string m_aCommand;
    }
}
