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
