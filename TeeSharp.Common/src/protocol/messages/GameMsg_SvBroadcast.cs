using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvBroadcast : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerBroadcast;

        public string Message { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Message);
            return packer.Error;
        }
    }
}