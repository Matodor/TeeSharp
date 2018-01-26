using System;

namespace TeeSharp.Common.Protocol
{
    public class NetObj_PlayerInput : BaseNetObject<NetObj_PlayerInput>
    {
        public int Direction;
        public int TargetX;
        public int TargetY;
        public int Jump;
        public int Fire;
        public int Hook;
        public int PlayerFlags;
        public int WantedWeapon;
        public int NextWeapon;
        public int PrevWeapon;

        public override bool Compare(NetObj_PlayerInput other)
        {
            return
                Direction == other.Direction &&
                TargetX == other.TargetX &&
                TargetY == other.TargetY &&
                Jump == other.Jump &&
                Fire == other.Fire &&
                Hook == other.Hook &&
                PlayerFlags == other.PlayerFlags &&
                WantedWeapon == other.WantedWeapon &&
                NextWeapon == other.NextWeapon &&
                PrevWeapon == other.PrevWeapon;
        }

        public override int[] Serialize()
        {
            return new[]
            {
                Direction,
                TargetX,
                TargetY,
                Jump,
                Fire,
                Hook,
                PlayerFlags,
                WantedWeapon,
                NextWeapon,
                PrevWeapon,
            };
        }

        public override void Deserialize(int[] data)
        {
            if (data.Length < 10)
                throw new Exception("Deserialize NetObj_PlayerInput error");

            Direction = data[0];
            TargetX = data[1];
            TargetY = data[2];
            Jump = data[3];
            Fire = data[4];
            Hook = data[5];
            PlayerFlags = data[6];
            WantedWeapon = data[7];
            NextWeapon = data[8];
            PrevWeapon = data[9];
        }
    }
}