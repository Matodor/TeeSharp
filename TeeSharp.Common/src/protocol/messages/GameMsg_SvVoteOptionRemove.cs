using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteOptionRemove : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_VOTEOPTIONREMOVE;

        public string Description;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Description);
            return packer.Error;
        }
    }
}