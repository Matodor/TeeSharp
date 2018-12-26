using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClStartInfo : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ClientStartInfo;

        public string Name { get; set; }
        public string Clan { get; set; }
        public int Country { get; set; }

        public string[] SkinPartNames { get; private set; }
        public bool[] UseCustomColors { get; private set; }
        public int[] SkinPartColors { get; private set; }

        public GameMsg_ClStartInfo()
        {
            SkinPartNames = new string[6];
            UseCustomColors = new bool[6];
            SkinPartColors = new int[6];
        }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Name);
            packer.AddString(Clan);
            packer.AddInt(Country);
            packer.AddString(SkinPartNames);
            packer.AddBool(UseCustomColors);
            packer.AddInt(SkinPartColors);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Name = unpacker.GetString(Sanitize);
            Clan = unpacker.GetString(Sanitize);
            Country = unpacker.GetInt();
            unpacker.GetString(SkinPartNames, Sanitize);
            unpacker.GetBool(UseCustomColors);
            unpacker.GetInt(SkinPartColors);

            return unpacker.Error;
        }
    }
}