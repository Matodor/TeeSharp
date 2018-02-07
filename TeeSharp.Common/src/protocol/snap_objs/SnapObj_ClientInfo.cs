using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Core;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_ClientInfo : BaseSnapObject
    {
        public override SnapshotItem Type { get; } = SnapshotItem.OBJ_CLIENTINFO;
        public override int SerializeLength { get; } = 17;

        public string Name;
        public string Clan;
        public int Country;
        public string Skin;
        public bool UseCustomColor;
        public int ColorBody;
        public int ColorFeet;

        public override int[] Serialize()
        {
            var name = Name.StrToInts(4);
            var clan = Clan.StrToInts(3);
            var skin = Skin.StrToInts(6);
            
            return new []
            {
                name[0],
                name[1],
                name[2],
                name[3],
                clan[0],
                clan[1],
                clan[2],
                Country,
                skin[0],
                skin[1],
                skin[2],
                skin[3],
                skin[4],
                skin[5],
                UseCustomColor ? 1 : 0,
                ColorBody,
                ColorFeet
            };
        }
    }
}