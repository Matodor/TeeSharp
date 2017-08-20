using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp.Server
{
    public abstract class IGameContext : ISingleton
    {
        public Player[] Players { get; }

        protected IGameContext()
        {
            Players = new Player[Consts.MAX_CLIENTS];
        }
    }
}
