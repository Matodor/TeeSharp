using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvChat : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_CHAT;

        public bool IsTeam;
        public int ClientId;
        public string Message;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(IsTeam ? 1 : 0);
            packer.AddInt(ClientId);
            packer.AddString(Message);
            return packer.Error;
        }
    }
}