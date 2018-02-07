using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_SpectatorInfo : BaseSnapObject
    {
        public override SnapshotItem Type { get; } = SnapshotItem.OBJ_SPECTATORINFO;
        public override int SerializeLength { get; } = 3;

        public int SpectatorId;
        public Vec2 ViewPos;

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