using System;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Game;
using TeeSharp.Core;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameWorld : BaseInterface
    {
        public abstract event Action Reseted;
        
        public virtual bool Paused { get; set; }
        public virtual WorldCore WorldCore { get; set; }
        public virtual bool ResetRequested { get; set; }

        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }

        public abstract Character IntersectCharacter(Vector2 pos1, Vector2 pos2, 
            float radius, ref Vector2 newPos, Character notThis);

        protected abstract void Reset();

        public abstract void Tick();
        public abstract void BeforeSnapshot();
        public abstract void OnSnapshot(int snappingClient);
        public abstract void AfterSnapshot();
    }
}