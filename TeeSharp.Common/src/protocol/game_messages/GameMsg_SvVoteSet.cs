using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteSet : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_VOTESET;

        public int Timeout;
        public string Description;
        public string Reason;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(Timeout);
            packer.AddString(Description);
            packer.AddString(Reason);
            return packer.Error;
        }
    }
}