using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvKillMsg : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_KILLMSG;

        public int Killer { get; set; }
        public int Victim { get; set; }
        public Weapons Weapon { get; set; }
        public int ModeSpecial { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddInt(Killer);
            packer.AddInt(Victim);
            packer.AddInt((int) Weapon);
            packer.AddInt(ModeSpecial);
            return packer.Error;
        }
    }
}