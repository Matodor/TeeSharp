using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_SpectatorInfo : BaseSnapObject
    {
        public override SnapObj Type { get; } = SnapObj.OBJ_SPECTATORINFO;
        public override int FieldsCount { get; } = 3;

        public int SpectatorId;
        public Vector2 ViewPos;

        public override int[] Serialize()
        {
            return new[]
            {
                SpectatorId,
                (int) ViewPos.x,
                (int) ViewPos.y,
            };
        }
    }
}