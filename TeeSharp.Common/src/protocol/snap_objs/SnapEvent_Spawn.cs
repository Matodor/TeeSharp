using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapEvent_Spawn : BaseSnapEvent
    {
        public override SnapshotItem Type { get; } = SnapshotItem.EVENT_SPAWN;
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