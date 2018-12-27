using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvKillMsg : BaseGameMessage, IClampedMaxClients
    {
        public override GameMessage Type => GameMessage.ServerKillMessage;

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

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Killer = unpacker.GetInt();
            Victim = unpacker.GetInt();
            Weapon = unpacker.GetInt();
            ModeSpecial = unpacker.GetInt();

            if (Killer < 0)
                failedOn = nameof(Killer);
            if (Victim < 0)
                failedOn = nameof(Victim);
            if (Weapon < -3 || Weapon >= (int) Enums.Weapon.NumWeapons)
                failedOn = nameof(Weapon);

            return unpacker.Error;
        }

        public void Validate(int maxClients, ref string failedOn)
        {
            if (Killer < 0 || Killer >= maxClients)
                failedOn = nameof(Killer);
            if (Victim < 0 || Victim >= maxClients)
                failedOn = nameof(Victim);
        }
    }
}