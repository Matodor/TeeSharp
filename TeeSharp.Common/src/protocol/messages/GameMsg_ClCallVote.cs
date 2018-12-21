using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClCallVote : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ClientCallVote;

        public string VoteType { get; set; }
        public string Value { get; set; }
        public string Reason { get; set; }
        public bool Force { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(VoteType);
            packer.AddString(Value);
            packer.AddString(Reason);
            packer.AddBool(Force);
            return packer.Error;
        }
    }
}