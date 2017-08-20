using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp.Server
{
    public class GameContext : IGameContext
    {
        protected GameContext()
        {
        }

        public static GameContext Create()
        {
            return new GameContext();
        }
    }
}
