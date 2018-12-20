using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvMotd : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_MOTD;

        public string Message;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Message);
            return packer.Error;
        }
    }
}