using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvChat : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_CHAT;

        public bool IsTeam { get; set; }
        public int ClientId { get; set; }
        public string Message { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddInt(IsTeam ? 1 : 0);
            packer.AddInt(ClientId);
            packer.AddString(Message);
            return packer.Error;
        }
    }
}