using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TeeSharp
{
    public class NetworkBan
    {
        public void BanAddr(IPEndPoint addr, int seconds, string reason)
        {
            
        }

        public bool IsBanned(IPEndPoint addr, out string reason)
        {
            reason = "";
            return false;
        }
    }
}
