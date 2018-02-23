using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    // not used
    public class SnapEvent_SoundGlobal : BaseSnapEvent
    {
        public override SnapshotItem Type { get; } = SnapshotItem.EVENT_SOUNDGLOBAL;
        public override int SerializeLength { get; } = 3;

        public Sound Sound;

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