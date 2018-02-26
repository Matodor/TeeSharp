using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapEvent_Death : BaseSnapEvent
    {
        public override SnapObject Type { get; } = SnapObject.EVENT_DEATH;
        public override int SerializeLength { get; } = 3;

        public int ClientId;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Position = new Vec2(
                data[dataOffset + 0],
                data[dataOffset + 1]
            );

            ClientId = data[dataOffset + 2];
        }

        public override int[] Serialize()
        {
            return new[]
            {
                Math.RoundToInt(Position.x),
                Math.RoundToInt(Position.y),
                ClientId
            };
        }
    }
}