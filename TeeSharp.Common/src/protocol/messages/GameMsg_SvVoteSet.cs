using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteSet : BaseGameMessage, IClampedMaxClients
    {
        public override GameMessage Type => GameMessage.ServerVoteSet;

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

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            ClientID = unpacker.GetInt();
            VoteType = (Vote) unpacker.GetInt();
            Timeout = unpacker.GetInt();
            Description = unpacker.GetString(Sanitize);
            Reason = unpacker.GetString(Sanitize);

            if (VoteType < 0 || VoteType >= Vote.NumTypes)
                failedOn = nameof(VoteType);
            if (Timeout < 0 || Timeout > 60)
                failedOn = nameof(Timeout);

            return unpacker.Error;
        }

        public void Validate(int maxClients, ref string failedOn)
        {
            if (ClientID < -1 || ClientID >= maxClients)
                failedOn = nameof(ClientID);
        }
    }
}