using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapEvent_HammerHit : BaseSnapEvent
    {
        public override SnapshotItem Type { get; } = SnapshotItem.EVENT_HAMMERHIT;
        public override int SerializeLength { get; } = 2;

        public override int[] Serialize()
        {
            return new[]
            {
                Math.RoundToInt(Position.x),
                Math.RoundToInt(Position.y),
            };
        }
    }
}