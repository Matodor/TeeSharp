using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvMotd : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerMotd;

        public string Message { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Message);
            return packer.Error;
        }
    }
}