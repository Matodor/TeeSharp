using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game.Entities
{
    public class Pickup : Entity<Pickup>
    {
        public override float ProximityRadius { get; protected set; } = 14f;

        private readonly Powerup _powerup;
        private readonly Weapon _weapon;

        public Pickup(Powerup powerup, Weapon weapon) : base(1)
        {
            _powerup = powerup;
            _weapon = weapon;
        }
        
        public override void OnSnapshot(int snappingClient)
        {
            if (NetworkClipped(snappingClient))
                return;

            var pickup = Server.SnapObject<SnapObj_Pickup>(IDs[0]);
            if (pickup == null)
                return;

            pickup.Position = Position;
            pickup.Powerup = _powerup;
            pickup.Weapon = _weapon;
        }
    }
}