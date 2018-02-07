using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_GameInfo : BaseSnapObject
    {
        public override SnapshotItem Type { get; } = SnapshotItem.OBJ_GAMEINFO;
        public override int SerializeLength { get; } = 8;

        public GameFlags GameFlags = GameFlags.NONE;
        public GameStateFlags GameStateFlags = GameStateFlags.NONE;
        public int RoundStartTick;
        public int WarmupTimer;
        public int ScoreLimit;
        public int TimeLimit;
        public int RoundNum;
        public int RoundCurrent;
        
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
    }
}