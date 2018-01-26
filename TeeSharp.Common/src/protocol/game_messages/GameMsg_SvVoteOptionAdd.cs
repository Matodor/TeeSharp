using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteOptionAdd : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_VOTEOPTIONADD;

        public string Description { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddString(Description);
            return packer.Error;
        }
    }
}