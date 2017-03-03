using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Teecsharp
{
    public delegate bool JOBFUNC(object pData);
    public class CJob
    {
        public const int
            STATE_PENDING = 0,
            STATE_RUNNING = 1,
            STATE_DONE = 2;

        public CJobPool m_pPool;
        public CJob m_pPrev;
        public CJob m_pNext;

        public volatile int m_Status;
        public volatile bool m_Result;

        public JOBFUNC m_pfnFunc;
        public object m_pFuncData;

        public CJob()
        {
            m_Status = STATE_DONE;
            m_pFuncData = null;
        }

        public int Status()
        {
            return m_Status;
        }

        public bool Result()
        {
            return m_Result;
        }
    }

    public class CJobPool
    {
        object m_Lock = new object();
        CJob m_pFirstJob;
        CJob m_pLastJob;

        void WorkerThread(CJobPool pPool)
        {
            while (true)
            {
                CJob pJob = null;

                // fetch job from queue
                lock (m_Lock)
                {
                    if (pPool.m_pFirstJob != null)
                    {
                        pJob = pPool.m_pFirstJob;
                        pPool.m_pFirstJob = pPool.m_pFirstJob.m_pNext;
                        if (pPool.m_pFirstJob != null)
                            pPool.m_pFirstJob.m_pPrev = null;
                        else
                            pPool.m_pLastJob = null;
                    }
                }

                // do the job if we have one
                if (pJob != null)
                {
                    pJob.m_Status = CJob.STATE_RUNNING;
                    pJob.m_Result = pJob.m_pfnFunc(pJob.m_pFuncData);
                    pJob.m_Status = CJob.STATE_DONE;
                }
                else
                    Thread.Sleep(10);
            }
        }

        public CJobPool()
        {
            // empty the pool
            m_pFirstJob = null;
            m_pLastJob = null;
        }

        public int Init(int NumThreads)
        {
            // start threads
            for (int i = 0; i < NumThreads; i++)
            {
                Thread thread = new Thread(() => { WorkerThread(this); });
                thread.Start();
            }
            return 0;
        }

        public int Add(CJob pJob, JOBFUNC pfnFunc, object pData)
        {
            pJob.m_pfnFunc = pfnFunc;
            pJob.m_pFuncData = pData;

            lock (m_Lock)
            {
                // add job to queue
                pJob.m_pPrev = m_pLastJob;
                if (m_pLastJob != null)
                    m_pLastJob.m_pNext = pJob;
                m_pLastJob = pJob;
                if (m_pFirstJob == null)
                    m_pFirstJob = pJob;
            }
            return 0;
        }
    }
}
