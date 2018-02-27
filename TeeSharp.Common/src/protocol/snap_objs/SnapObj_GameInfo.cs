using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_GameInfo : BaseSnapObject
    {
        public override SnapObject Type { get; } = SnapObject.OBJ_GAMEINFO;
        public override int SerializeLength { get; } = 8;

        public GameFlags GameFlags = GameFlags.NONE;
        public GameStateFlags GameStateFlags = GameStateFlags.NONE;
        public int RoundStartTick;
        public int WarmupTimer;
        public int ScoreLimit;
        public int TimeLimit;
        public int RoundNum;
        public int RoundCurrent;

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            GameFlags = (GameFlags) data[dataOffset + 0];
            GameStateFlags = (GameStateFlags) data[dataOffset + 1];
            RoundStartTick = data[dataOffset + 2];
            WarmupTimer = data[dataOffset + 3];
            ScoreLimit = data[dataOffset + 4];
            TimeLimit = data[dataOffset + 5];
            RoundNum = data[dataOffset + 6];
            RoundCurrent = data[dataOffset + 7];
        }

        public override int[] Serialize()
        {
            return new[]
            {
                (int) GameFlags,
                (int) GameStateFlags,
                RoundStartTick,
                WarmupTimer,
                ScoreLimit,
                TimeLimit,
                RoundNum,
                RoundCurrent,
            };
        }

        public override string ToString()
        {
            return $"SnapObj_GameInfo gameFlags={GameFlags} gameStateFlags={GameStateFlags}" +
                   $" roundStartTick={RoundStartTick} warmupTimer={WarmupTimer} scoreLimit={ScoreLimit}" +
                   $" timeLimit={TimeLimit} roundNum={RoundNum} roundCurrent={RoundCurrent}";
        }
    }
}