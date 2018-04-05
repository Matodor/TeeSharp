using System.Net;

namespace TeeSharp.MasterServer
{
    public class ServerInfo
    {
        public class PlayerInfo
        {
            public string Name { get; set; }
            public string Clan { get; set; }
            public CountryInfo Country { get; set; }
            public int Score { get; set; }
            public bool InGame { get; set; }
        }

        public PlayerInfo[] Players { get; set; }
        public IPEndPoint Ip { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public string Map { get; set; }
        public string GameType { get; set; }
        public int NumPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int NumClients { get; set; }
        public int MaxClients { get; set; }
    }
}