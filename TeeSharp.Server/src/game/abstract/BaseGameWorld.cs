using System;
using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Game;
using TeeSharp.Core;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameWorld : BaseInterface
    {
        public virtual bool Paused { get; set; }
        public virtual WorldCore WorldCore { get; set; }

        protected virtual BaseTuningParams Tuning { get; set; }
        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }
        
        public abstract T FindEntity<T>(Predicate<T> predicate) where T : Entity<T>;
        public abstract IEnumerable<T> GetEntities<T>() where T : Entity<T>; 
        public abstract IEnumerable<T> FindEntities<T>(Vector2 pos, float radius) where T : Entity<T>; 
        public abstract IEnumerable<T> FindEntities<T>(Predicate<T> predicate) where T : Entity<T>; 
        public abstract T ClosestEntity<T>(Vector2 pos, float radius, T notThis) where T : Entity<T>;

        public abstract Character IntersectCharacter(Vector2 pos1, Vector2 pos2, float radius, ref Vector2 newPos, Character notThis);
        public abstract void Reset();

        public abstract void Tick();
        public abstract void BeforeSnapshot();
        public abstract void OnSnapshot(int snappingClient);
        public abstract void AfterSnapshot();
    }
}