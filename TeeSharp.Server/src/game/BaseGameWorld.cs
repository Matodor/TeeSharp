using System;
using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Game;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameWorld : BaseInterface
    {
        public virtual bool IsPaused { get; set; }
        public virtual WorldCore WorldCore { get; set; }

        protected virtual BaseTuningParams Tuning { get; set; }
        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }
        protected virtual IList<Entity> Entities { get; set; }

        protected BaseGameWorld()
        {
            GameContext = Kernel.Get<BaseGameContext>();
            Server = Kernel.Get<BaseServer>();
            Config = Kernel.Get<BaseConfig>();
            Tuning = Kernel.Get<BaseTuningParams>();
        }

        public abstract T FindEntity<T>(Predicate<Entity<T>> predicate) where T : Entity<T>;

        public abstract IEnumerable<T> GetEntities<T>() where T : Entity<T>; 
        public abstract IEnumerable<T> FindEntities<T>(Vec2 pos, float radius) where T : Entity<T>; 
        public abstract IEnumerable<T> FindEntities<T>(Predicate<Entity<T>> predicate) where T : Entity<T>; 
        
        public abstract void AddEntity<T>(Entity<T> entity) where T : Entity<T>;
        public abstract void RemoveEntity<T>(Entity<T> entity) where T : Entity<T>;

        public abstract void Reset();
        public abstract void RemoteEntities();

        public abstract void Tick();
        public abstract void OnSnapshot(int snappingClient);
    }
}