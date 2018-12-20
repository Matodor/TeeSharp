using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    // not used
    public class SnapEvent_SoundGlobal : BaseSnapEvent
    {
        public override SnapshotObjects Type => = SnapshotObjects.EVENT_SOUNDGLOBAL;
        public override int SerializeLength { get; } = 3;

        public Sound Sound;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Position = new Vector2(
                data[dataOffset + 0],
                data[dataOffset + 1]
            );

            Sound = (Sound) data[dataOffset + 2];
        }

        public override int[] Serialize()
        {
            return new[]
            {
                MathHelper.RoundToInt(Position.x),
                MathHelper.RoundToInt(Position.y),
                (int) Sound
            };
        }

        public override string ToString()
        {
            return $"SnapEvent_SoundGlobal pos={Position} sound={Sound}";
        }
    }
}