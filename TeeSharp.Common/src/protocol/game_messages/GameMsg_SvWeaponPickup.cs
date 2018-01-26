using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvWeaponPickup : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_WEAPONPICKUP;

        public Weapons Weapon { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddInt((int) Weapon);
            return packer.Error;
        }
    }
}