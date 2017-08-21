using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
