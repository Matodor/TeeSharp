using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClSay : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ClientSay;

        public ChatMode ChatMode { get; set; }
        public int TargetId { get; set; }
        public string Message { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) ChatMode);
            packer.AddInt(TargetId);
            packer.AddString(Message);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            ChatMode = (ChatMode) unpacker.GetInt();
            TargetId = unpacker.GetInt();
            Message = unpacker.GetString(Sanitize);

            if (ChatMode < 0 || ChatMode >= ChatMode.NumModes)
                failedOn = nameof(ChatMode);

            return unpacker.Error;
        }
    }
}