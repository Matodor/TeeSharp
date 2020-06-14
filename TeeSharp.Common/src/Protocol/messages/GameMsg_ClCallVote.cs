using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClCallVote : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ClientCallVote;

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

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            VoteType = unpacker.GetString(Sanitize);
            Value = unpacker.GetString(Sanitize);
            Reason = unpacker.GetString(Sanitize);
            Force = unpacker.GetBool();

            return unpacker.Error;
        }
    }
}