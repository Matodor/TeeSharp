using System;

namespace Teecsharp
{
    public class CHostLookup
    {
        public CJob m_Job = new CJob();
        public string m_aHostname;
        public int m_Nettype;
        public NETADDR m_Addr = new NETADDR();
    }

    public abstract class IEngine : IInterface
    {   public abstract void Init();
        public abstract void InitLogfile();
        public abstract void HostLookup(CHostLookup pLookup, string pHostname, int Nettype);
        public abstract void AddJob(CJob pJob, JOBFUNC pfnFunc, object pData);

        protected CJobPool m_JobPool;
    }
}
