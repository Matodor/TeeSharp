using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    // not used
    public class SnapEvent_SoundGlobal : BaseSnapEvent
    {
        public override SnapObject Type { get; } = SnapObject.EVENT_SOUNDGLOBAL;
        public override int SerializeLength { get; } = 3;

        public Sound Sound;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Position = new Vec2(
                data[dataOffset + 0],
                data[dataOffset + 1]
            );

            Sound = (Sound) data[dataOffset + 2];
        }

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