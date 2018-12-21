using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvGameMsg : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerGameMessage;

        public override bool PackError(MsgPacker packer)
        {
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            return unpacker.Error;
        }
    }
}