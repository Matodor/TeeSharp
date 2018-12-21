using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvMotd : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerMotd;

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