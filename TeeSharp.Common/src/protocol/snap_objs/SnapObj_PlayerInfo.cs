using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_PlayerInfo : BaseSnapObject
    {
        public override SnapObject Type { get; } = SnapObject.OBJ_PLAYERINFO;
        public override int SerializeLength { get; } = 5;

        public bool Local;
        public int ClientId;
        public Team Team;
        public int Score;
        public int Latency;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Local = data[dataOffset + 0] != 0;
            ClientId = data[dataOffset + 1];
            Team = (Team) data[dataOffset + 2];
            Score = data[dataOffset + 3];
            Latency = data[dataOffset + 4];
        }

        public override int[] Serialize()
        {
            return new[]
            {
                Local ? 1 : 0,
                ClientId,
                (int) Team,
                Score,
                Latency,
            };
        }

        public override string ToString()
        {
            return $"SnapObj_PlayerInfo local={Local} clientId={ClientId} " +
                   $" team={Team} score={Score} latency={Latency}";
        }
    }
}