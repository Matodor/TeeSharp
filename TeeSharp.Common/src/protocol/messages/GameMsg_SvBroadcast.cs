using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvBroadcast : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerBroadcast;

        public string Message { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Message);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Message = unpacker.GetString(Sanitize);
            return unpacker.Error;
        }
    }
}