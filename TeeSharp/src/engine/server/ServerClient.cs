using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp.Server
{
    public class ServerClient : IServerClient
    {
        public ConsoleAccessLevel AccessLevel { get; set; }
        public ServerClientState ClientState { get; set; }
        public string Name { get; set; }
        public string Clan { get; set; }
        public int Country { get; set; }
        public int AuthTries { get; set; }
        public long Traffic { get; set; }
        public long TrafficSince { get; set; }

        public SnapshotStorage SnapshotStorage { get; }

        public ServerClient()
        {
            SnapshotStorage = new SnapshotStorage();
        }

        public void Reset()
        {
            
        }
    }
}
