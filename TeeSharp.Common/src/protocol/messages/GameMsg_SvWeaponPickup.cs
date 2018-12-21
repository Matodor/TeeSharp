using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvWeaponPickup : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerWeaponPickup;

        public Weapon Weapon { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) Weapon);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Weapon = (Weapon) unpacker.GetInt();

            if (Weapon < 0 || Weapon >= Weapon.NumWeapons)
                failedOn = nameof(Weapon);

            return unpacker.Error;
        }
    }
}