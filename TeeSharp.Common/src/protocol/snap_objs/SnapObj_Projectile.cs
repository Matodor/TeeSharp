using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_Projectile : BaseSnapObject
    {
        public override SnapObject Type { get; } = SnapObject.OBJ_PROJECTILE;
        public override int SerializeLength { get; } = 6;

        public Vec2 Position;
        public Vec2 Velocity;
        public int StartTick;
        public Weapon Weapon;

        public void FillMsgPacker(MsgPacker msg)
        {
            msg.AddInt((int) Position.x);
            msg.AddInt((int) Position.y);
            msg.AddInt((int) Velocity.x);
            msg.AddInt((int) Velocity.y);
            msg.AddInt((int) Weapon);
            msg.AddInt(StartTick);
        }

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Position = new Vec2(
                data[dataOffset + 0],
                data[dataOffset + 1]
            );

            Velocity = new Vec2(
                data[dataOffset + 2],
                data[dataOffset + 3]
            );

            Weapon = (Weapon) data[dataOffset + 4];
            StartTick = data[dataOffset + 5];
        }

        public override int[] Serialize()
        {
            return new []
            {
                (int) Position.x,
                (int) Position.y,
                (int) Velocity.x,
                (int) Velocity.y,
                (int) Weapon,
                StartTick
            };
        }
    }
}