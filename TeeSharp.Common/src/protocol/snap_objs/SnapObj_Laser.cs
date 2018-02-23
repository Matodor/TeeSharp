using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_Laser : BaseSnapObject
    {
        public override SnapshotItem Type { get; } = SnapshotItem.OBJ_LASER;
        public override int SerializeLength { get; } = 5;

        public Vec2 Position;
        public Vec2 From;
        public int StartTick;

        public override int[] Serialize()
        {
            return new[]
            {
                Math.RoundToInt(Position.x),
                Math.RoundToInt(Position.y),
                Math.RoundToInt(From.x),
                Math.RoundToInt(From.y),
                StartTick   
            };
        }
    }
}