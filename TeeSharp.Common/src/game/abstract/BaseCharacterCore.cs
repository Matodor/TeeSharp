using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;

namespace TeeSharp.Common
{
    public abstract class BaseCharacterCore : BaseInterface
    {
        public const float TeeSize = 28.0f;

        public virtual bool IsPredicted { get; set; }
        public virtual Vector2 Position { get; set; }
        public virtual Vector2 Velocity { get; set; }

        public virtual Vector2 HookPosition { get; set; }
        public virtual Vector2 HookDirection { get; set; }
        public virtual int HookTick { get; set; }
        public virtual HookState HookState { get; set; }
        public virtual int HookedPlayer { get; set; }

        public virtual int Jumped { get; set; }
        public virtual int Direction { get; set; }
        public virtual int Angle { get; set; }
        public virtual CoreEvents TriggeredEvents { get; set; }

        protected virtual WorldCore World { get; set; }
        protected virtual BaseMapCollision MapCollision { get; set; }
        protected virtual SnapshotCharacter QuantizeCore { get; set; }

        public abstract void Init(WorldCore worldCore, BaseMapCollision mapCollision);
        public abstract void Reset();
        public abstract void Tick(SnapshotPlayerInput input);
        public abstract void Move();
        public abstract void Write(SnapshotCharacterCore core);
        public abstract void Quantize();
        public abstract void Read(SnapshotCharacter core);
    }
}