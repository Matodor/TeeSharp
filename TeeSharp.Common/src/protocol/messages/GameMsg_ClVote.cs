using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClVote : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_VOTE;

        public int Vote;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(Vote);
            return packer.Error;
        }
    }
}