using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvClientInfo : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerClientInfo;

        public int ClientID { get; set; }
        public bool Local { get; set; }
        public Team Team { get; set; }
        public string Name { get; set; }
        public string Clan { get; set; }
        public int Country { get; set; }

        public string[] SkinPartNames { get; private set; }
        public bool[] UseCustomColors { get; private set; }
        public int[] SkinPartColors { get; private set; }

        public bool Silent { get; set; }

        public GameMsg_SvClientInfo()
        {
            SkinPartNames = new string[6];
            UseCustomColors = new bool[6];
            SkinPartColors = new int[6];
        }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(ClientID);
            packer.AddBool(Local);
            packer.AddInt((int) Team);
            packer.AddString(Name);
            packer.AddString(Clan);
            packer.AddInt(Country);
            packer.AddString(SkinPartNames);
            packer.AddBool(UseCustomColors);
            packer.AddInt(SkinPartColors);
            packer.AddBool(Silent);

            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            ClientID = unpacker.GetInt();
            Local = unpacker.GetBool();
            Team = (Team) unpacker.GetInt();
            Name = unpacker.GetString(Sanitize);
            Clan = unpacker.GetString(Sanitize);
            Country = unpacker.GetInt();

            unpacker.GetString(SkinPartNames, Sanitize);
            unpacker.GetBool(UseCustomColors);
            unpacker.GetInt(SkinPartColors);

            Silent = unpacker.GetBool();

            if (ClientID < 0)
                failedOn = nameof(ClientID);
            if (Team < Team.Spectators || Team > Team.Blue)
                failedOn = nameof(Team);

            return unpacker.Error;
        }
    }
}