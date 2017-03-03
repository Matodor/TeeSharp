using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    class CConfig : IConfig
    {
        IStorage m_pStorage;
        FileStream m_ConfigFile;

        class CCallback
        {
            public Action<IConfig, object> m_pfnFunc;
            public object m_pUserData;
        }

        const int MAX_CALLBACKS = 16;
        CCallback[] m_aCallbacks = new CCallback[MAX_CALLBACKS];
        int m_NumCallbacks;

        /*private void EscapeParam(char* pDst, const char* pSrc, int size)
	    {
		    for(int i = 0; *pSrc && i<size - 1; ++i)
		    {
			    if(*pSrc == '"' || *pSrc == '\\') // escape \ and "
				    *pDst++ = '\\';
			    *pDst++ = *pSrc++;
		    }
            * pDst = 0;
        }*/

        public CConfig()
        {
            for (int i = 0; i < m_aCallbacks.Length; i++)
                m_aCallbacks[i] = new CCallback();
            m_ConfigFile = null;
            m_NumCallbacks = 0;
        }

        public static IConfig CreateConfig()
        {
            return new CConfig();
        }

        public override void Init()
        {
            m_pStorage = Kernel.RequestInterface<IStorage>();
            Reset();
        }

        public override void Reset()
        {

        }

        public override void RestoreStrings()
        {

        }

        public override void Save()
        {
            if (m_pStorage == null)
                return;
            m_ConfigFile = m_pStorage.OpenFile("settings.cfg", CSystem.IOFLAG_WRITE, IStorage.TYPE_SAVE);

            if (m_ConfigFile == null)
                return;

            for (int i = 0; i < m_NumCallbacks; i++)
                m_aCallbacks[i].m_pfnFunc(this, m_aCallbacks[i].m_pUserData);

            CSystem.io_close(m_ConfigFile);
            m_ConfigFile = null;
        }

        public override void RegisterCallback(Action<IConfig, object> pfnFunc, object pUserData)
        {
            m_aCallbacks[m_NumCallbacks].m_pfnFunc = pfnFunc;
            m_aCallbacks[m_NumCallbacks].m_pUserData = pUserData;
            m_NumCallbacks++;
        }

        public override void WriteLine(string pLine)
        {
            if (m_ConfigFile == null)
                return;

            CSystem.io_write(m_ConfigFile, pLine);
            CSystem.io_write_newline(m_ConfigFile);
        }
    }
}
