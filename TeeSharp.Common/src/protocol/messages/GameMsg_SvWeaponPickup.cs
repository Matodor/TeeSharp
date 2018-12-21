using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvWeaponPickup : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerWeaponPickup;

        public Weapon Weapon { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) Weapon);
            return packer.Error;
        }
    }
}