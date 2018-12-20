using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClChangeInfo : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_CHANGEINFO;

        public string Name;
        public string Clan;
        public int Country;
        public string Skin;
        public bool UseCustomColor;
        public int ColorBody;
        public int ColorFeet;

        public override bool PackError(MsgPacker packer)
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