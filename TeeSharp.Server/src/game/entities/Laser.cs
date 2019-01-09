using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game.Entities
{
    public class Laser : Entity<Laser>
    {
        public override float ProximityRadius { get; protected set; }

        private Vector2 _direction;
        private readonly int _owner;
        private float _energy;
        private int _bounces;
        private int _evalTick;
        private Vector2 _from;

        public Laser(Vector2 direction, float startEnergy, int owner) : base(1)
        {
            _direction = direction;
            _energy = startEnergy;
            _owner = owner;
            _bounces = 0;
            _evalTick = 0;

            Reseted += OnReseted;
            DoBounce();
        }

        private void OnReseted(Entity entity)
        {
            Destroy();
        }

        protected virtual bool HitCharacter(Vector2 from, Vector2 to)
        {
            var hitAt = Vector2.Zero;
            var ownerCharacter = GameContext.Players[_owner]?.GetCharacter();
            var hitCharacter = GameWorld.IntersectCharacter(Position, to, 0f, ref hitAt, ownerCharacter);

            if (hitCharacter == null)
                return false;

            _from = from;
            _energy = -1;
            Position = hitAt;

            hitCharacter.TakeDamage(
                force: Vector2.Zero, 
                source: (to - from).Normalized, 
                damage: ServerData.Weapons.Laser.Damage, 
                @from: _owner,
                weapon: Weapon.Laser);

            return true;
        }

        protected virtual void DoBounce()
        {
            if (_energy < 0)
            {
                Destroy();
                return;
            }

            _evalTick = Server.Tick;
            var to = Position + _direction * _energy;
            if (GameContext.MapCollision.IntersectLine(Position, to, out _, out to).HasFlag(CollisionFlags.Solid))
            {
                if (!HitCharacter(Position, to))
                {
                    _from = Position;
                    Position = to;

                    var tempPos = Position;
                    var tempDirection = _direction;

                    GameContext.MapCollision.MovePoint(ref tempPos, ref tempDirection, 1f, out _);
                    Position = tempPos;
                    _direction = tempDirection.Normalized;

                    _energy -= MathHelper.Distance(_from, Position) + Tuning["LaserBounceCost"];
                    _bounces++;

                    if (_bounces > Tuning["LaserBounceNum"])
                        _energy = -1;

                    GameContext.CreateSound(Position, Sound.LaserBounce);
                }
            }
            else
            {
                if (!HitCharacter(Position, to))
                {
                    _from = Position;
                    _energy = -1;
                    Position = to;
                }
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (Server.Tick > _evalTick + (Server.TickSpeed * Tuning["LaserBounceDelay"]) / 1000f)
                DoBounce();
        }

        public override void TickPaused()
        {
            base.TickPaused();

            _evalTick++;
        }

        public override void OnSnapshot(int snappingClient)
        {
            if (NetworkClipped(snappingClient) && NetworkClipped(snappingClient, _from))
                return;

            var laser = Server.SnapshotItem<SnapshotLaser>(IDs[0]);
            if (laser == null)
                return;

            laser.StartTick = _evalTick;
            laser.FromX = (int) _from.x;
            laser.FromY = (int) _from.y;
            laser.X = (int) Position.x;
            laser.Y = (int) Position.y;
        }
    }
}
