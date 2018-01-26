using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteSet : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_VOTESET;

        public int Timeout { get; set; }
        public string Description { get; set; }
        public string Reason { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddInt(Timeout);
            packer.AddString(Description);
            packer.AddString(Reason);
            return packer.Error;
        }
    }
}