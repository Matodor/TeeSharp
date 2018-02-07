using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapEvent_DamageInd : BaseSnapEvent
    {
        public override SnapshotItem Type { get; } = SnapshotItem.EVENT_DAMAGEIND;
        public override int SerializeLength { get; } = 3;

        public int Angle;

        public override int[] Serialize()
        {
            return new[]
            {
                Math.RoundToInt(Position.x),
                Math.RoundToInt(Position.y),
                Angle
            };
        }
    }
}