using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public class CKernel : IKernel
    {
        private class CInterfaceInfo
        {
            public CInterfaceInfo()
            {
                m_aName = "";
                m_pInterface = null;
            }

            public string m_aName;
            public IInterface m_pInterface;
        };
        const int MAX_INTERFACES = 32;
        CInterfaceInfo[] m_aInterfaces = new CInterfaceInfo[MAX_INTERFACES];
        int m_NumInterfaces;

        private CInterfaceInfo FindInterfaceInfo(string pName)
        {
            for (int i = 0; i < m_NumInterfaces; i++)
            {
                if (pName == m_aInterfaces[i].m_aName)
                    return m_aInterfaces[i];
            }
            return null;
        }

        public CKernel()
        {
            m_NumInterfaces = 0;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool RegisterInterfaceImpl(string InterfaceName, IInterface pInterface)
        {
            if (pInterface == null)
            {
                CSystem.dbg_msg("kernel", "ERROR: couldn't register interface {0}. null pointer given",
                    InterfaceName);
                return false;
            }

            if (m_NumInterfaces == MAX_INTERFACES)
            {
                CSystem.dbg_msg("kernel", "ERROR: couldn't register interface '{0}'. maximum of interfaces reached",
                    InterfaceName);
                return false;
            }

            if (FindInterfaceInfo(InterfaceName) != null)
            {
                CSystem.dbg_msg("kernel", "ERROR: couldn't register interface '{0}'. interface already exists",
                    InterfaceName);
                return false;
            }

            pInterface.SetKernel(this);
            m_aInterfaces[m_NumInterfaces] = new CInterfaceInfo();
            m_aInterfaces[m_NumInterfaces].m_pInterface = pInterface;
            m_aInterfaces[m_NumInterfaces].m_aName = InterfaceName;
            m_NumInterfaces++;

            return true;
        }

        public override bool ReregisterInterfaceImpl(string InterfaceName, IInterface pInterface)
        {
            if (FindInterfaceInfo(InterfaceName) == null)
            {
                CSystem.dbg_msg("kernel", "ERROR: couldn't reregister interface '{0}'. interface doesn't exist",
                    InterfaceName);
                return false;
            }

            pInterface.SetKernel(this);
            return true;
        }

        public override T RequestInterfaceImpl<T>(string InterfaceName)
        {
            CInterfaceInfo pInfo = FindInterfaceInfo(InterfaceName);
            if (pInfo == null)
            {
                CSystem.dbg_msg("kernel", "failed to find interface with the name '{0}'", InterfaceName);
                return null;
            }
            return (T)pInfo.m_pInterface;
        }

        public static IKernel Create()
        {
            return new CKernel();
        }
    }
}
