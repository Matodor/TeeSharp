using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_SpectatorInfo : BaseSnapObject
    {
        public override SnapObject Type { get; } = SnapObject.OBJ_SPECTATORINFO;
        public override int SerializeLength { get; } = 3;

        public int SpectatorId;
        public Vec2 ViewPos;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            SpectatorId = data[dataOffset + 0];
            ViewPos = new Vec2(
                data[dataOffset + 1],
                data[dataOffset + 2]
            );
        }

        public override int[] Serialize()
        {
            return new[]
            {
                SpectatorId,
                Math.RoundToInt(ViewPos.x),
                Math.RoundToInt(ViewPos.y),
            };
        }
    }
}