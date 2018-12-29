using System;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class Entity<T> : Entity where T : Entity<T>
    {
        /// <summary>
        /// Entities of type <see cref="T"/>
        /// </summary>
        public static readonly BidirectionalList<T> Entities;

        private BidirectionalList<T>.Node _node;

        static Entity()
        {
            Entities = BidirectionalList<T>.New();
        }

        protected Entity(int idsCount) : base(idsCount)
        {
            _node = Entities.Add((T) this);
            Destroyed += OnDestroyed;
        }

        private void OnDestroyed(Entity obj)
        {
            Entities.RemoveFast(_node);
            _node = null;
        }
    }

    public delegate void EntityEvent(Entity entity);
    public abstract class Entity : BaseInterface
    {
        public event EntityEvent Destroyed;
        public event EntityEvent Reseted;

        /// <summary>
        /// All entities on map
        /// </summary>
        public static readonly BidirectionalList<Entity> All;

        public abstract float ProximityRadius { get; protected set; }
        public virtual Vector2 Position { get; set; }

        protected virtual BaseTuningParams Tuning { get; set; }
        protected virtual BaseGameWorld GameWorld { get; set; }
        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }
        protected virtual BaseGameConsole Console { get; set; }

        protected virtual int[] IDs { get; set; }

        private BidirectionalList<Entity>.Node _node;

        static Entity()
        {
            All = BidirectionalList<Entity>.New();
        }

        protected Entity(int idsCount)
        {
            _node = All.Add(this);

            GameContext = Kernel.Get<BaseGameContext>();
            Server = Kernel.Get<BaseServer>();
            GameWorld = Kernel.Get<BaseGameWorld>();
            Config = Kernel.Get<BaseConfig>();
            Tuning = Kernel.Get<BaseTuningParams>();
            Console = Kernel.Get<BaseGameConsole>();

            IDs = new int[idsCount];
            for (var i = 0; i < IDs.Length; i++)
                IDs[i] = Server.SnapshotNewId();

            Position = Vector2.zero;
        }

        public abstract void OnSnapshot(int snappingClient);
        public virtual void Tick() { }
        public virtual void LateTick() { }
        public virtual void TickPaused() { }

        public void Reset()
        {
            Reseted?.Invoke(this);
        }

        public void Destroy()
        {
            if (_node == null)
                return;

            Destroyed?.Invoke(this);

            for (var i = 0; i < IDs.Length; i++)
                Server.SnapshotFreeId(IDs[i]);

            All.RemoveFast(_node);
            _node = null;
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

            if (Math.Abs(dx) > 1000f || Math.Abs(dy) > 800.0f)
                return true;

            return MathHelper.Distance(GameContext.Players[snappingClient].ViewPos, checkPos) > 1100.0f;
        }

        public bool GameLayerClipped(Vector2 checkPos)
        {
            var rx = MathHelper.RoundToInt(checkPos.x) / 32;
            var ry = MathHelper.RoundToInt(checkPos.y) / 32;

            return (rx < -200 || MathHelper.RoundToInt(checkPos.x) / 32 > GameContext.MapCollision.Width + 200) ||
                   (ry < -200 || MathHelper.RoundToInt(checkPos.y) / 32 > GameContext.MapCollision.Height + 200);
        }
    }
}