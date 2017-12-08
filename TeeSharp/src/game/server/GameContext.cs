using System;

namespace TeeSharp.Server
{
    public class GameContext : IGameContext
    {
        private readonly IPlayer[] _players;

        public GameContext()
        {
            _players = new IPlayer[Consts.MAX_CLIENTS];
        }

        public IPlayer GetPlayer(int clientId)
        {
            return _players[clientId];
        }

        public bool IsClientPlayer(int ClientID)
        {
            return _players[ClientID] != null && _players[ClientID].Team != Teams.SPECTATORS;
        }

        public bool IsClientReady(int ClientID)
        {
            return true;
        }

        public string GameType()
        {
            return "test";
        }

        public void OnInit()
        {
            
        }

        public void OnShutdown()
        {
        }

        public void OnConsoleInit()
        {
        }

        public void OnClientEnter(int clientId)
        {
        }

        public void OnClientConnected(int clientId)
        {
        }

        public void OnClientDrop(int clientId, string reason)
        {
        }

        public void OnMessage(NetMessages msg, Unpacker unpacker, int clientId)
        {
        }

        public void OnClientPredictedInput(int clientId, int[] data)
        {
        }

        public void OnClientDirectInput(int clientId, int[] data)
        {
        }

        public void OnTick()
        {
        }
    }
}
