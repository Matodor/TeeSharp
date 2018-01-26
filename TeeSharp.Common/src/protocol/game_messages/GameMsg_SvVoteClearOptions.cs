using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteClearOptions : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_VOTECLEAROPTIONS;

        public override bool Pack(MsgPacker packer)
        {
            return packer.Error;
        }
    }
}