using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;
using TeeSharp.Core;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_ClientInfo : BaseSnapObject
    {
        public override SnapObject Type { get; } = SnapObject.OBJ_CLIENTINFO;
        public override int SerializeLength { get; } = 17;

        public string Name;
        public string Clan;
        public int Country;
        public string Skin;
        public bool UseCustomColor;
        public int ColorBody;
        public int ColorFeet;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Name = new []
            {
                data[dataOffset + 0],
                data[dataOffset + 1],
                data[dataOffset + 2],
                data[dataOffset + 3],
            }.IntsToStr();

            Clan = new[]
            {
                data[dataOffset + 4],
                data[dataOffset + 5],
                data[dataOffset + 6],
            }.IntsToStr();
            
            Country = data[dataOffset + 7];

            Skin = new[]
            {
                data[dataOffset + 8],
                data[dataOffset + 9],
                data[dataOffset + 10],
                data[dataOffset + 11],
                data[dataOffset + 12],
                data[dataOffset + 13],
            }.IntsToStr();

            UseCustomColor = data[dataOffset + 14] == 1;
            ColorBody = data[dataOffset + 15];
            ColorFeet = data[dataOffset + 16];
        }

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

        public override string ToString()
        {
            return $"SnapObj_ClientInfo name={Name} clan={Clan} country={Country}" +
                   $" skin={Skin} useCustomColor={UseCustomColor} colorBody={ColorBody} colorFeet={ColorFeet}";
        }
    }
}