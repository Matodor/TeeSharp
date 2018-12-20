using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvWeaponPickup : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_WEAPONPICKUP;

        public Weapon Weapon;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) Weapon);
            return packer.Error;
        }
    }
}