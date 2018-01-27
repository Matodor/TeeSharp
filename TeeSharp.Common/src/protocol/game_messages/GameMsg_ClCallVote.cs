using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClCallVote : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_CALLVOTE;

        public string Type;
        public string Value;
        public string Reason;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Type);
            packer.AddString(Value);
            packer.AddString(Reason);
            return packer.Error;
        }
    }
}