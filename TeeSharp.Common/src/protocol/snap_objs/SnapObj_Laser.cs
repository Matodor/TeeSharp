using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_Laser : BaseSnapObject
    {
        public override SnapObject Type { get; } = SnapObject.OBJ_LASER;
        public override int SerializeLength { get; } = 5;

        public Vector2 Position;
        public Vector2 From;
        public int StartTick;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Position = new Vector2(
                data[dataOffset + 0],
                data[dataOffset + 1]
            );

            From = new Vector2(
                data[dataOffset + 2],
                data[dataOffset + 3]
            );

            StartTick = data[dataOffset + 4];
        }

        public override int[] Serialize()
        {
            return new[]
            {
                MathHelper.RoundToInt(Position.x),
                MathHelper.RoundToInt(Position.y),
                MathHelper.RoundToInt(From.x),
                MathHelper.RoundToInt(From.y),
                StartTick   
            };
        }

        public override string ToString()
        {
            return $"SnapObj_Laser pos={Position} from={From} startTick={StartTick}";
        }
    }
}