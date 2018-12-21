using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClSetTeam : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ClientSetTeam;

        public Team Team { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) Team);
            return packer.Error;
        }
    }
}