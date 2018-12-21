using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteStatus : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerVoteStatus;

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
    }
}