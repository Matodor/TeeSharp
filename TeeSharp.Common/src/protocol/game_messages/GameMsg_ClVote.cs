using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClVote : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_VOTE;

        public int Vote { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddInt(Vote);
            return packer.Error;
        }
    }
}