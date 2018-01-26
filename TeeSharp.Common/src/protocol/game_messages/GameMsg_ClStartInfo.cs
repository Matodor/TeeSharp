using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClStartInfo : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_STARTINFO;

        public string Name { get; set; }
        public string Clan { get; set; }
        public int Country { get; set; }
        public string Skin { get; set; }
        public int UseCustomColor { get; set; }
        public int ColorBody { get; set; }
        public int ColorFeet { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddString(Name);
            packer.AddString(Clan);
            packer.AddInt(Country);
            packer.AddString(Skin);
            packer.AddInt(UseCustomColor);
            packer.AddInt(ColorBody);
            packer.AddInt(ColorFeet);
            return packer.Error;
        }
    }
}