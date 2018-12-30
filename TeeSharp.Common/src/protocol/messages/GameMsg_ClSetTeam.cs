using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClSetTeam : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ClientSetTeam;

        public Team Team { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) Team);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Team = (Team) unpacker.GetInt();

            if (Team < Team.Spectators || Team > Team.Blue)
                failedOn = nameof(Team);

            return unpacker.Error;
        }
    }
}