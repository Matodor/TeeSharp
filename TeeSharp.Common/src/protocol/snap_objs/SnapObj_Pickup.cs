using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_Pickup : BaseSnapObject
    {
        public override SnapObj Type { get; } = SnapObj.OBJ_PICKUP;
        public override int SerializeLength { get; } = 4;

        public vec2 Position;
        public Powerup Powerup;
        public Weapon Weapon;

        public override int[] Serialize()
        {
            return new []
            {
                (int) Position.x,
                (int) Position.y,
                (int) Powerup,
                (int) Weapon
            };
        }
    }
}