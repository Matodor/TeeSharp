using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class Entity : BaseInterface
    {
        public abstract float ProximityRadius { get; protected set; }
        public virtual Vector2 Position { get; set; }
        public virtual bool MarkedForDestroy { get; private set; }

        protected virtual BaseTuningParams Tuning { get; set; }
        protected virtual BaseGameWorld GameWorld { get; set; }
        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }
        protected virtual int[] IDs { get; set; }

        public abstract void OnSnapshot(int snappingClient);

        protected Entity(int idsCount)
        {
            GameContext = Kernel.Get<BaseGameContext>();
            Server = Kernel.Get<BaseServer>();
            GameWorld = Kernel.Get<BaseGameWorld>();
            Config = Kernel.Get<BaseConfig>();
            Tuning = Kernel.Get<BaseTuningParams>();

            IDs = new int[idsCount];
            for (var i = 0; i < IDs.Length; i++)
                IDs[i] = Server.SnapshotNewId();

            Position = Vector2.zero;
        }

        public virtual void Tick() { }
        public virtual void TickDefered() { }
        public virtual void TickPaused() { }
        public virtual void OnDestroy() { }
        public virtual void Reset() { }

        public virtual void Destroy()
        {
            if (MarkedForDestroy)
                return;

            MarkedForDestroy = true;
            OnDestroy();

            for (var i = 0; i < IDs.Length; i++)
                Server.SnapshotFreeId(IDs[i]);
        }

        public virtual bool NetworkClipped(int snappingClient)
        {
            return NetworkClipped(snappingClient, Position);
        }

        public virtual bool NetworkClipped(int snappingClient, Vector2 checkPos)
        {
            if (snappingClient == -1)
                return false;

            var dx = GameContext.Players[snappingClient].ViewPos.x - checkPos.x;
            var dy = GameContext.Players[snappingClient].ViewPos.y - checkPos.y;

            if (System.Math.Abs(dx) > 900.0f ||
                System.Math.Abs(dy) > 700.0f)
            {
                return true;
            }

            return Common.Math.Distance(GameContext.Players[snappingClient].ViewPos, checkPos) > 1100.0f;
        }

        public bool GameLayerClipped(Vector2 checkPos)
        {
            return Math.RoundToInt(checkPos.x) / 32 < -200 ||
                   Math.RoundToInt(checkPos.x) / 32 > GameContext.Collision.Width + 200 ||
                   Math.RoundToInt(checkPos.y) / 32 < -200 ||
                   Math.RoundToInt(checkPos.y) / 32 > GameContext.Collision.Height + 200;
        }
    }

    public abstract class Entity<T> : Entity where T : Entity<T>
    {
        public static Entity<T> FirstTypeEntity { get; set; }
        public virtual Entity<T> NextTypeEntity { get; set; } 
        public virtual Entity<T> PrevTypeEntity { get; set; } 

        protected Entity(int idsCount) : base(idsCount)
        {
            NextTypeEntity = null;
            PrevTypeEntity = null;
            GameWorld.AddEntity(this);
        }

        public override void OnDestroy()
        {
            GameWorld.RemoveEntity(this);
            base.OnDestroy();
        }
    }
}