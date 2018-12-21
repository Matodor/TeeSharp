using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvSettings : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerSettings;

        public bool KickVote { get; set; }
        public int KickMin { get; set; }
        public bool SpecVote { get; set; }
        public bool TeamLock { get; set; }
        public bool TeamBalance { get; set; }
        public int PlayerSlots { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddBool(KickVote);
            packer.AddInt(KickMin);
            packer.AddBool(SpecVote);
            packer.AddBool(TeamLock);
            packer.AddBool(TeamBalance);
            packer.AddInt(PlayerSlots);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            KickVote = unpacker.GetBool();
            KickMin = unpacker.GetInt();
            SpecVote = unpacker.GetBool();
            TeamLock = unpacker.GetBool();
            TeamBalance = unpacker.GetBool();
            PlayerSlots = unpacker.GetInt();

            if (KickMin < 0)
                failedOn = nameof(KickMin);
            if (PlayerSlots < 0)
                failedOn = nameof(PlayerSlots);

            return unpacker.Error;
        }
    }
}