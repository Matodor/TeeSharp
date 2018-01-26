using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvBroadcast : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_BROADCAST;

        public string Message { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddString(Message);
            return packer.Error;
        }
    }
}