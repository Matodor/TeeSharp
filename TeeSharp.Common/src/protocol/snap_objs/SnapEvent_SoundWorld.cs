using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapEvent_SoundWorld : BaseSnapEvent
    {
        public override SnapObject Type { get; } = SnapObject.EVENT_SOUNDWORLD;
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

        public override string ToString()
        {
            return $"SnapEvent_SoundWorld pos={Position} sound={Sound}";
        }
    }
}