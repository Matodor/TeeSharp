using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_Projectile : BaseSnapObject
    {
        public override SnapshotItem Type { get; } = SnapshotItem.OBJ_PROJECTILE;
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