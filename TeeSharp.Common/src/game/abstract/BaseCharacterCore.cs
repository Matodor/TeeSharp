using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;

namespace TeeSharp.Common
{
    public class BaseCharacterCore : BaseInterface
    {
        public const float TeeSize = 28.0f;

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
        public virtual CoreEventFlags TriggeredEvents { get; set; }
        public virtual SnapObj_PlayerInput Input { get; set; }

        protected virtual WorldCore World { get; set; }
        protected virtual BaseCollision Collision { get; set; }
        protected virtual SnapObj_Character QuantizeCore { get; set; }
    }
}