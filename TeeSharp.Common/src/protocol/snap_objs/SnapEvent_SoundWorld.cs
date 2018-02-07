using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapEvent_SoundWorld : BaseSnapEvent
    {
        public override SnapshotItem Type { get; } = SnapshotItem.EVENT_SOUNDWORLD;
        public override int SerializeLength { get; } = 3;

        public Sounds Sound;

        public override int[] Serialize()
        {
            return new[]
            {
                Math.RoundToInt(Position.x),
                Math.RoundToInt(Position.y),
                (int) Sound
            };
        }
    }
}