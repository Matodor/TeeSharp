using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapEvent_Explosion : BaseSnapEvent
    {
        public override SnapshotObjects Type => = SnapshotObjects.EVENT_EXPLOSION;
        public override int SerializeLength { get; } = 2;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Position = new Vector2(
                data[dataOffset + 0],
                data[dataOffset + 1]
            );
        }

        public override int[] Serialize()
        {
            return new[]
            {
                MathHelper.RoundToInt(Position.x),
                MathHelper.RoundToInt(Position.y),
            };
        }

        public override string ToString()
        {
            return $"SnapEvent_Explosion pos={Position}";
        }
    }
}