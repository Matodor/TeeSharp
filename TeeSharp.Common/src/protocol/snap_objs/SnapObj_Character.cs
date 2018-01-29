using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_Character : BaseSnapObject
    {
        public override SnapObj Type { get; } = SnapObj.OBJ_CHARACTER;
        public override int SerializeLength { get; } = 22;

        public int Tick;
        public Vec2 Position;
        public Vec2 Velocity;
        public int Angle;
        public int Direction;
        public int Jumped;
        public int HookedPlayer;
        public int HookState;
        public int HookTick;
        public Vec2 HookPosition;
        public Vec2 HookDirection;
        public PlayerFlags PlayerFlags = PlayerFlags.NONE;
        public int Health;
        public int Armor;
        public int AmmoCount;
        public Weapon Weapon = Weapon.HAMMER;
        public Emote Emote = Emote.NORMAL;
        public int AttackTick;

        public bool Compare(SnapObj_Character other)
        {
            return
                Tick == other.Tick &&
                Math.RoundToInt(Position.x) == Math.RoundToInt(other.Position.x) &&
                Math.RoundToInt(Position.y) == Math.RoundToInt(other.Position.y) &&
                Math.RoundToInt(Velocity.x) == Math.RoundToInt(other.Velocity.x) &&
                Math.RoundToInt(Velocity.y) == Math.RoundToInt(other.Velocity.y) &&

                Angle == other.Angle &&
                Direction == other.Direction &&
                Jumped == other.Jumped &&
                HookedPlayer == other.HookedPlayer &&
                HookState == other.HookState &&
                HookTick == other.HookTick &&

                Math.RoundToInt(HookPosition.x) == Math.RoundToInt(other.HookPosition.x) &&
                Math.RoundToInt(HookPosition.y) == Math.RoundToInt(other.HookPosition.y) &&
                Math.RoundToInt(HookDirection.x) == Math.RoundToInt(other.HookDirection.x) &&
                Math.RoundToInt(HookDirection.y) == Math.RoundToInt(other.HookDirection.y) &&

                PlayerFlags == other.PlayerFlags &&
                Health == other.Health &&
                Armor == other.Armor &&
                AmmoCount == other.AmmoCount &&
                Weapon == other.Weapon &&
                Emote == other.Emote &&
                AttackTick == other.AttackTick;
        }

        public override int[] Serialize()
        {
            return new[]
            {
                Tick,
                Math.RoundToInt(Position.x),
                Math.RoundToInt(Position.y),
                Math.RoundToInt(Velocity.x),
                Math.RoundToInt(Velocity.y),
                Angle,
                Direction,
                Jumped,
                HookedPlayer,
                HookState,
                HookTick,
                Math.RoundToInt(HookPosition.x),
                Math.RoundToInt(HookPosition.y),
                Math.RoundToInt(HookDirection.x),
                Math.RoundToInt(HookDirection.y),
                (int) PlayerFlags,
                Health,
                Armor,
                AmmoCount,
                (int) Weapon,
                (int) Emote,
                AttackTick,
            };
        }
    }
}