using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvGameInfo : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerGameInfo;

        public GameFlags GameFlags { get; set; }
        public int ScoreLimit { get; set; }
        public int TimeLimit { get; set; }
        public int MatchNum { get; set; }
        public int MatchCurrent { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) GameFlags);
            packer.AddInt(ScoreLimit);
            packer.AddInt(TimeLimit);
            packer.AddInt(MatchNum);
            packer.AddInt(MatchCurrent);
            return packer.Error;
        }
    }
}