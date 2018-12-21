using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvChat : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerChat;

        public ChatMode ChatMode { get; set; }
        public int ClientId { get; set; }
        public int TargetId { get; set; }
        public string Message { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) ChatMode);
            packer.AddInt(ClientId);
            packer.AddInt(TargetId);
            packer.AddString(Message);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            ChatMode = (ChatMode) unpacker.GetInt();
            ClientId = unpacker.GetInt();
            TargetId = unpacker.GetInt();
            Message = unpacker.GetString(Sanitize);

            if (ChatMode < 0 || ChatMode >= ChatMode.NumModes)
                failedOn = nameof(ChatMode);

            return unpacker.Error;
        }
    }
}