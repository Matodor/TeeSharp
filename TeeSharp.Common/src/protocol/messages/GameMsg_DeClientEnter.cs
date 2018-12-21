using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_DeClientEnter : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.DemoClientEnter;

        public string Name { get; set; }
        public int ClientId { get; set; }
        public Team Team { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Name);
            packer.AddInt(ClientId);
            packer.AddInt((int) Team);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Name = unpacker.GetString(Sanitize);
            ClientId = unpacker.GetInt();
            Team = (Team) unpacker.GetInt();

            if (Team < Team.Spectators || Team > Team.Blue)
                failedOn = nameof(Team);

            return unpacker.Error;
        }
    }
}