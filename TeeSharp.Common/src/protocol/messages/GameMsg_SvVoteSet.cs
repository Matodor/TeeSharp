using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteSet : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerVoteSet;

        public int ClientID { get; set; }
        public Vote VoteType { get; set; }
        public int Timeout { get; set; }
        public string Description { get; set; }
        public string Reason { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(ClientID);
            packer.AddInt((int) VoteType);
            packer.AddInt(Timeout);
            packer.AddString(Description);
            packer.AddString(Reason);
            return packer.Error;
        }
    }
}