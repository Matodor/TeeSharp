using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public class CServerBan : CNetBan
    {

    }

    public class CNetBan
    {
        public int BanAddr(NETADDR pAddr, int Seconds, string pReason)
        {
            //return Ban(&m_BanAddrPool, pAddr, Seconds, pReason);
            return 0;
        }

        public bool IsBanned(NETADDR addr, string aBuf)
        {
            return false;
        }
    }
}
