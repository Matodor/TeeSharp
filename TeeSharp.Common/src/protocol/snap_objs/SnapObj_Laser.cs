using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_Laser : BaseSnapObject
    {
        public override SnapObject Type { get; } = SnapObject.OBJ_LASER;
        public override int SerializeLength { get; } = 5;

        public Vec2 Position;
        public Vec2 From;
        public int StartTick;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Position = new Vec2(
                data[dataOffset + 0],
                data[dataOffset + 1]
            );

            From = new Vec2(
                data[dataOffset + 2],
                data[dataOffset + 3]
            );

            StartTick = data[dataOffset + 4];
        }

        public override int[] Serialize()
        {
            return new[]
            {
                Math.RoundToInt(Position.x),
                Math.RoundToInt(Position.y),
                Math.RoundToInt(From.x),
                Math.RoundToInt(From.y),
                StartTick   
            };
        }

        public override string ToString()
        {
            return $"SnapObj_Laser pos={Position} from={From} startTick={StartTick}";
        }
    }
}