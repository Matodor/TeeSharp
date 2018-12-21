using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvTeam : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerTeam;

        public int ClientId { get; set; }
        public Team Team { get; set; }
        public bool Silent { get; set; }
        public int CooldownTick { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(ClientId);
            packer.AddInt((int) Team);
            packer.AddBool(Silent);
            packer.AddInt(CooldownTick);
            return packer.Error;
        }
    }
}