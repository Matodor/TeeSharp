using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvKillMsg : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_KILLMSG;

        public int Killer;
        public int Victim;
        public Weapon Weapon;
        public int ModeSpecial;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(Killer);
            packer.AddInt(Victim);
            packer.AddInt((int) Weapon);
            packer.AddInt(ModeSpecial);
            return packer.Error;
        }
    }
}