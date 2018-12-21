using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClSay : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ClientSay;

        public Chat ChatMode { get; set; }
        public int TargetId { get; set; }
        public string Message { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) ChatMode);
            packer.AddInt(TargetId);
            packer.AddString(Message);
            return packer.Error;
        }
    }
}