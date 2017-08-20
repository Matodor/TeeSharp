using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public class NetworkServer : INetworkServer
    {
        protected NetworkServer()
        {
            
        }

        public static NetworkServer Create()
        {
            return new NetworkServer();
        }
    }
}
