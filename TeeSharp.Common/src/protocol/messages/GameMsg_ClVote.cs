using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClVote : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ClientVote;

        public int Vote { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(Vote);
            return packer.Error;
        }
    }
}