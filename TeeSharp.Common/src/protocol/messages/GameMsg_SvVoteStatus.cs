using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteStatus : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_VOTESTATUS;

        public int Yes;
        public int No;
        public int Pass;
        public int Total;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(Yes);
            packer.AddInt(No);
            packer.AddInt(Pass);
            packer.AddInt(Total);
            return packer.Error;
        }
    }
}