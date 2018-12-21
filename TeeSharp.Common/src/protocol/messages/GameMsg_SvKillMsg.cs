using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvKillMsg : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerKillMessage;

        public int Killer { get; set; }
        public int Victim { get; set; }
        public int Weapon { get; set; }
        public int ModeSpecial { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(Killer);
            packer.AddInt(Victim);
            packer.AddInt(Weapon);
            packer.AddInt(ModeSpecial);
            return packer.Error;
        }
    }
}