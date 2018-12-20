﻿using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_SpectatorInfo : BaseSnapObject
    {
        public override SnapObject Type { get; } = SnapObject.OBJ_SPECTATORINFO;
        public override int SerializeLength { get; } = 3;

        public int SpectatorId;
        public Vector2 ViewPos;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            SpectatorId = data[dataOffset + 0];
            ViewPos = new Vector2(
                data[dataOffset + 1],
                data[dataOffset + 2]
            );
        }

        public override int[] Serialize()
        {
            return new[]
            {
                SpectatorId,
                MathHelper.RoundToInt(ViewPos.x),
                MathHelper.RoundToInt(ViewPos.y),
            };
        }
    }
}