using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapEvent_DamageInd : BaseSnapEvent
    {
        public override SnapObject Type { get; } = SnapObject.EVENT_DAMAGEIND;
        public override int SerializeLength { get; } = 3;

        public int Angle;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Position = new Vector2(
                data[dataOffset + 0], 
                data[dataOffset + 1]
            );

            Angle = data[dataOffset + 2];
        }

        public override int[] Serialize()
        {
            return new[]
            {
                Math.RoundToInt(Position.x),
                Math.RoundToInt(Position.y),
                Angle
            };
        }

        public override string ToString()
        {
            return $"SnapEvent_DamageInd pos={Position} angle={Angle}";
        }
    }
}