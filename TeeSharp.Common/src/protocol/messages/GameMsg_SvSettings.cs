using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvSettings : BaseGameMessage, IClampedMaxClients
    {
        public override GameMessage Type => GameMessage.ServerSettings;

        public bool KickVote { get; set; }
        public int KickMin { get; set; }
        public bool SpectatorsVote { get; set; }
        public bool TeamLock { get; set; }
        public bool TeamBalance { get; set; }
        public int PlayerSlots { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddBool(KickVote);
            packer.AddInt(KickMin);
            packer.AddBool(SpectatorsVote);
            packer.AddBool(TeamLock);
            packer.AddBool(TeamBalance);
            packer.AddInt(PlayerSlots);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            KickVote = unpacker.GetBool();
            KickMin = unpacker.GetInt();
            SpectatorsVote = unpacker.GetBool();
            TeamLock = unpacker.GetBool();
            TeamBalance = unpacker.GetBool();
            PlayerSlots = unpacker.GetInt();
            return unpacker.Error;
        }

        public void Validate(int maxClients, ref string failedOn)
        {
            if (KickMin < 0 || KickMin > maxClients)
                failedOn = nameof(KickMin);

            if (PlayerSlots < 0 || PlayerSlots > maxClients)
                failedOn = nameof(PlayerSlots);
        }
    }
}