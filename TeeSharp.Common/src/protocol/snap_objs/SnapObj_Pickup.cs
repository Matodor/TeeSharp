using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_Pickup : BaseSnapObject
    {
        public override SnapObj Type { get; } = SnapObj.OBJ_PICKUP;
        public override int SerializeLength { get; } = 4;

        public Vec2 Position;
        public Powerup Powerup = Powerup.WEAPON;
        public Weapon Weapon = Weapon.HAMMER;

        public override int[] Serialize()
        {
            return new []
            {
                Math.RoundToInt(Position.x),
                Math.RoundToInt(Position.y),
                (int) Powerup,
                (int) Weapon
            };
        }
    }
}