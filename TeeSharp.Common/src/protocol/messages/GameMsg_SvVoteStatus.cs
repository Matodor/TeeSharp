using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteStatus : BaseGameMessage
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

            if (Yes < 0)
                failedOn = nameof(Yes);
            if (No < 0)
                failedOn = nameof(No);
            if (Pass < 0)
                failedOn = nameof(Pass);
            if (Total < 0)
                failedOn = nameof(Total);

            return unpacker.Error;
        }
    }
}