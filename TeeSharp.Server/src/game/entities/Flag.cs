using TeeSharp.Common.Enums;

namespace TeeSharp.Server.Game.Entities
{
    public class Flag : Entity<Flag>
    {
        public override float ProximityRadius { get; protected set; }

        public Character Carrier { get; protected set; }
        public Team Team { get; protected set; }

        public Flag() : base(idsCount: 1)
        {
        }

        public override void OnSnapshot(int snappingClient)
        {
        }
    }
}