using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClCallVote : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_CALLVOTE;

        public string Type { get; set; }
        public string Value { get; set; }
        public string Reason { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddString(Type);
            packer.AddString(Value);
            packer.AddString(Reason);
            return packer.Error;
        }
    }
}