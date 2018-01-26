using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvReadyToEnter : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_READYTOENTER;

        public override bool Pack(MsgPacker packer)
        {
            return packer.Error;
        }
    }
}