using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvSettings : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerSettings;

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
    }
}