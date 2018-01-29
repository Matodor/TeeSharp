using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_PlayerInfo : BaseSnapObject
    {
        public override SnapObj Type { get; } = SnapObj.OBJ_PLAYERINFO;
        public override int SerializeLength { get; } = 5;

        public int Local;
        public int ClientId;
        public Team Team;
        public int Score;
        public int Latency;

        public override int[] Serialize()
        {
            return new[]
            {
                Local,
                ClientId,
                (int) Team,
                Score,
                Latency,
            };
        }
    }
}