using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game.Entities
{
    public class Laser : Entity<Laser>
    {
        public override float ProximityRadius { get; protected set; } = 16f;

        private readonly int _ownerId;
        private Vector2 _direction;
        private Vector2 _from;
        private float _energy;
        private int _bounces;
        private int _evalTick;
    
        public Laser(Vector2 position, Vector2 direction, float startEnergy,
            int ownerId) : base(1)
        {
            Position = position;
            _direction = direction;
            _ownerId = ownerId;
            _energy = startEnergy;
            _bounces = 0;
            _evalTick = 0;
            
            DoBounce();
        }

        protected virtual void DoBounce()
        {
            _evalTick = Server.Tick;

            if (_energy < 0)
            {
                Destroy();
                return;
            }

            var to = Position + _direction * _energy;

            if (GameContext.Collision.IntersectLine(Position, to, out _, out to) != TileFlags.NONE)
            {
                if (!HitCharacter(Position, to))
                {
                    _from = Position;
                    Position = to;

                    var tempPos = Position;
                    var tempDir = _direction * 4f;

                    GameContext.Collision.MovePoint(ref tempPos, ref tempDir, 1f, out _);
                    Position = tempPos;
                    _direction = tempDir.Normalized;

                    _energy -= MathHelper.Distance(_from, Position) + Tuning["LaserBounceCost"];
                    _bounces++;

                    if (_bounces > Tuning["LaserBounceNum"])
                        _energy = -1;

                    GameContext.CreateSound(Position, Sound.LaserBounce);
                }
            }
            else if (!HitCharacter(Position, to))
            {
                _from = Position;
                _energy = -1;
                Position = to;
            }
        }

        public override void Reset()
        {
            Destroy();
        }

        public override void Tick()
        {
            if (Server.Tick > _evalTick + (Server.TickSpeed * Tuning["LaserBounceDelay"]) / 1000f)
                DoBounce();
        }

        public override void TickPaused()
        {
            _evalTick++;
        }

        protected virtual bool HitCharacter(Vector2 from, Vector2 to)
        {
            var hitAt = Vector2.zero;
            var ownerCharacter = GameContext.Players[_ownerId]?.GetCharacter();
            var hitCharacter = GameWorld.IntersectCharacter(Position, to, 0f, ref hitAt, ownerCharacter);

            if (hitCharacter == null)
                return false;

            _from = from;
            Position = hitAt;
            _energy = -1;
            hitCharacter.TakeDamage(Vector2.zero, (int)Tuning["LaserDamage"].FloatValue, 
                _ownerId, Weapon.Laser);
            return true;
        }

        public override void OnSnapshot(int snappingClient)
        {
            if (NetworkClipped(snappingClient))
                return;

            var laser = Server.SnapObject<SnapObj_Laser>(IDs[0]);
            if (laser == null)
                return;

            laser.Position = Position;
            laser.From = _from;
            laser.StartTick = _evalTick;
        }
    }
}