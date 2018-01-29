using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Core;
using Math = System.Math;

namespace TeeSharp.Server.Game
{
    public abstract class Entity : BaseInterface
    {
        public abstract float ProximityRadius { get; protected set; }
        public virtual Vec2 Position { get; set; }
        public virtual bool MarkedForDestroy { get; protected set; }

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

            IDs = new int[idsCount];
            for (var i = 0; i < IDs.Length; i++)
                IDs[i] = Server.SnapshotNewId();

            Position = Vec2.zero;
        }

        public virtual void Tick() { }
        public virtual void TickDefered() { }
        public virtual void TickPaused() { }
        public virtual void OnDestroy() { }

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

        public virtual bool NetworkClipped(int snappingClient, Vec2 checkPos)
        {
            if (snappingClient == -1)
                return false;

            var dx = GameContext.Players[snappingClient].ViewPos.x - checkPos.x;
            var dy = GameContext.Players[snappingClient].ViewPos.y - checkPos.y;

            if (Math.Abs(dx) > 900.0f ||
                Math.Abs(dy) > 700.0f)
            {
                return true;
            }

            return VectorMath.Distance(GameContext.Players[snappingClient].ViewPos, checkPos) > 1100.0f;
        }

        public bool GameLayerClipped(Vec2 checkPos)
        {
            return Common.Math.RoundToInt(checkPos.x) / 32 < -200 ||
                   Common.Math.RoundToInt(checkPos.x) / 32 > GameContext.Collision.Width + 200 ||
                   Common.Math.RoundToInt(checkPos.y) / 32 < -200 ||
                   Common.Math.RoundToInt(checkPos.y) / 32 > GameContext.Collision.Height + 200;
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