using System;

namespace TeeSharp.Server
{
    public class GameContext : IGameContext
    {
        private readonly Player[] _players;

        public GameContext()
        {
            _players = new Player[Consts.MAX_CLIENTS];
        }

        public Player GetPlayer(int clientId)
        {
            return _players[clientId];
        }

        public void OnInit()
        {
            throw new NotImplementedException();
        }

        public void OnShutdown()
        {
            throw new NotImplementedException();
        }

        public void OnTick()
        {
            throw new NotImplementedException();
        }
    }
}
