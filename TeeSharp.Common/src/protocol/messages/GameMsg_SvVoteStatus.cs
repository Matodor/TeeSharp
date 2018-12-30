using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteStatus : BaseGameMessage, IClampedMaxClients
    {
        public override GameMessage Type => GameMessage.ServerVoteStatus;

        public int Yes { get; set; }
        public int No { get; set; }
        public int Pass { get; set; }
        public int Total { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(Yes);
            packer.AddInt(No);
            packer.AddInt(Pass);
            packer.AddInt(Total);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Yes = unpacker.GetInt();
            No = unpacker.GetInt();
            Pass = unpacker.GetInt();
            Total = unpacker.GetInt();
            return unpacker.Error;
        }

        public void Validate(int maxClients, ref string failedOn)
        {
            if (Yes < 0 || Yes > maxClients)
                failedOn = nameof(Yes);
            if (No < 0 || No > maxClients)
                failedOn = nameof(No);
            if (Pass < 0 || Pass > maxClients)
                failedOn = nameof(Pass);
            if (Total < 0 || Total > maxClients)
                failedOn = nameof(Total);
        }
    }
}