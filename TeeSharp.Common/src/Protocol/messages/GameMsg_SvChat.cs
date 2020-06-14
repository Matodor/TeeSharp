using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvChat : BaseGameMessage, IClampedMaxClients
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

        public void Validate(int maxClients, ref string failedOn)
        {
            if (ClientId < -1 || ClientId >= maxClients)
                failedOn = nameof(ClientId);
            if (TargetId < -1 || TargetId >= maxClients)
                failedOn = nameof(TargetId);
        }
    }
}