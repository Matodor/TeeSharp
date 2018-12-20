﻿using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClKill : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_KILL;

        public override bool PackError(MsgPacker packer)
        {
            return packer.Error;
        }
    }
}