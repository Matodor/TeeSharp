using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvBroadcast : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_BROADCAST;

        public string Message;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Message);
            return packer.Error;
        }
    }
}