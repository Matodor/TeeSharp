using System;
using System.Collections.Generic;
using System.Net;

namespace TeeSharp.MasterServer
{
    public class ServersSnapshotItem
    {
        public DateTime? LastUpdateInfo { get; set; }
        public ServerInfo ServerInfo { get; set; }
    }

    public class ServersSnapshot
    {
        public int ServersCount { get; private set; }
        public int ServersOnline { get; private set; }

        private readonly Dictionary<IPEndPoint, ServersSnapshotItem> _servers;

        public ServersSnapshot()
        {
            _servers = new Dictionary<IPEndPoint, ServersSnapshotItem>();
        }

        public void UpdateServerInfo(IPEndPoint serverAddr, ServerInfo info)
        {
            if (!_servers.ContainsKey(serverAddr))
                return;

            ServersOnline++;

            _servers[serverAddr].LastUpdateInfo = DateTime.Now;
            _servers[serverAddr].ServerInfo = info;
        }

        public void AddServer(IPEndPoint serverAddr)
        {
            if (_servers.ContainsKey(serverAddr))
                return;

            ServersCount++;

            _servers.Add(serverAddr, new ServersSnapshotItem
            {
                LastUpdateInfo = null,
            });
        }
    }
}