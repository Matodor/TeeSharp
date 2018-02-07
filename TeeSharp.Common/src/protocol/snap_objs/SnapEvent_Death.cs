using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapEvent_Death : BaseSnapEvent
    {
        public override SnapshotItem Type { get; } = SnapshotItem.EVENT_DEATH;
        public override int SerializeLength { get; } = 3;

        public int ClientId;

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