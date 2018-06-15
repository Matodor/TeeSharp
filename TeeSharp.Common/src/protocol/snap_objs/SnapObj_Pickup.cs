using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_Pickup : BaseSnapObject
    {
        public override SnapObject Type { get; } = SnapObject.OBJ_PICKUP;
        public override int SerializeLength { get; } = 4;

        public Vector2 Position;
        public Powerup Powerup = Powerup.WEAPON;
        public Weapon Weapon = Weapon.HAMMER;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Position = new Vector2(
                data[dataOffset + 0],
                data[dataOffset + 1]
            );

            Powerup = (Powerup) data[dataOffset + 2];
            Weapon = (Weapon) data[dataOffset + 3];
        }

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

        public override string ToString()
        {
            return $"SnapObj_Pickup pos={Position} powerup={Powerup} weapon={Weapon}";
        }
    }
}