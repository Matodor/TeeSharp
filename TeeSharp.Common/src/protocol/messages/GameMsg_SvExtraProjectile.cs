﻿using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvExtraProjectile : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_EXTRAPROJECTILE;

        public override bool PackError(MsgPacker packer)
        {
            return packer.Error;
        }
    }
}