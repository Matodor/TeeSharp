using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvGameInfo : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerGameInfo;

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

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            GameFlags = (GameFlags) unpacker.GetInt();
            ScoreLimit = unpacker.GetInt();
            TimeLimit = unpacker.GetInt();
            MatchNum = unpacker.GetInt();
            MatchCurrent = unpacker.GetInt();

            if (GameFlags < 0)
                failedOn = nameof(GameFlags);
            if (ScoreLimit < 0)
                failedOn = nameof(ScoreLimit);
            if (TimeLimit < 0)
                failedOn = nameof(TimeLimit);
            if (MatchNum < 0)
                failedOn = nameof(MatchNum);
            if (MatchCurrent < 0)
                failedOn = nameof(MatchCurrent);

            return unpacker.Error;
        }
    }
}