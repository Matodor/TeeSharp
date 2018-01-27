using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClChangeInfo : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_CHANGEINFO;

        public string Name { get; set; }
        public string Clan { get; set; }
        public int Country { get; set; }
        public string Skin { get; set; }
        public bool UseCustomColor { get; set; }
        public int ColorBody { get; set; }
        public int ColorFeet { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddString(Name);
            packer.AddString(Clan);
            packer.AddInt(Country);
            packer.AddString(Skin);
            packer.AddInt(UseCustomColor ? 1 : 0);
            packer.AddInt(ColorBody);
            packer.AddInt(ColorFeet);
            return packer.Error;
        }
    }
}