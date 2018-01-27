using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClSay : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_SAY;

        public bool IsTeam { get; set; }
        public string Message { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddInt(IsTeam ? 1 : 0);
            packer.AddString(Message);
            return packer.Error;
        }
    }
}