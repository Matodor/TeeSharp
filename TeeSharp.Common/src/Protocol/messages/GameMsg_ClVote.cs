using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClVote : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ClientVote;

        public int Vote { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(Vote);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Vote = unpacker.GetInt();

            if (Vote < -1 || Vote > 1)
                failedOn = nameof(Vote);

            return unpacker.Error;
        }
    }
}