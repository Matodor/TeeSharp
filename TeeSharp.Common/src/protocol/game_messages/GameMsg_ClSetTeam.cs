using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClSetTeam : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_SETTEAM;

        public int Team { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddInt(Team);
            return packer.Error;
        }
    }
}