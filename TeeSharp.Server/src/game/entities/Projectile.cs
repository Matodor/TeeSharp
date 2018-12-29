using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game.Entities
{
    public class Projectile : Entity<Projectile>
    {
        private readonly Weapon _weapon;
        private readonly int _ownerId;
        private readonly Vector2 _direction;
        private readonly int _damage;
        private readonly bool _explosive;
        private readonly float _force;
        private readonly Sound _soundImpact;
        private int _startTick;
        private int _lifeSpan;

        public override float ProximityRadius { get; protected set; }

        public Projectile(Weapon weapon, int ownerId, Vector2 direction,
            int lifeSpan, int damage, bool explosive, float force, Sound soundImpact) : base(1)
        {
            _weapon = weapon;
            _ownerId = ownerId;
            _direction = direction;
            _lifeSpan = lifeSpan;
            _damage = damage;
            _explosive = explosive;
            _force = force;
            _soundImpact = soundImpact;
            _startTick = Server.Tick;

            Reseted += OnReseted;
        }

        private void OnReseted(Entity entity)
        {
            Destroy();
        }

        public override void Tick()
        {
            base.Tick();

            var prevTime = (Server.Tick - _startTick - 1) / (float)Server.TickSpeed;
            var currentTime = (Server.Tick - _startTick) / (float)Server.TickSpeed;
            var prevPos = GetPos(prevTime);
            var currentPos = GetPos(currentTime);

            var collideFlags = GameContext.MapCollision.IntersectLine(prevPos, currentPos, out var collisionPos, out _);
            var ownerCharacter = GameContext.Players[_ownerId]?.GetCharacter();
            var targetCharacter = GameWorld.IntersectCharacter(prevPos, currentPos, 6.0f, ref currentPos, ownerCharacter);

            _lifeSpan--;

            if (_lifeSpan < 0 || 
                targetCharacter != null || 
                collideFlags.HasFlag(CollisionFlags.Solid) || 
                GameLayerClipped(currentPos))
            {
                if (_lifeSpan >= 0 || _weapon == Weapon.Grenade)
                    GameContext.CreateSound(currentPos, _soundImpact);

                if (_explosive)
                    GameContext.CreateExplosion(currentPos, _ownerId, _weapon, _damage);
                else
                {
                    targetCharacter?.TakeDamage(
                        force: _direction * System.Math.Max(0.001f, _force), 
                        damage: _damage, 
                        from: _ownerId,
                        weapon: _weapon);
                }

                Destroy();
            }
        }

        public override void TickPaused()
        {
            base.TickPaused();

            _startTick++;
        }

        protected Vector2 GetPos(float t)
        {
            var curvature = 0f;
            var speed = 0f;

            switch (_weapon)
            {
                case Weapon.Gun:
                    curvature = Tuning["GunCurvature"];
                    speed = Tuning["GunSpeed"];
                    break;

                case Weapon.Shotgun:
                    curvature = Tuning["ShotgunCurvature"];
                    speed = Tuning["ShotgunSpeed"];
                    break;

                case Weapon.Grenade:
                    curvature = Tuning["GrenadeCurvature"];
                    speed = Tuning["GrenadeSpeed"];
                    break;
            }

            return MathHelper.CalcPos(Position, _direction, curvature, speed, t);
        }


        public override void OnSnapshot(int snappingClient)
        {
            var currentTime = (Server.Tick - _startTick) / (float) Server.TickSpeed;

            if (NetworkClipped(snappingClient, GetPos(currentTime)))
                return;

            var projectile = Server.SnapshotItem<SnapshotProjectile>(IDs[0]);
            if (projectile == null)
                return;

            projectile.X = (int) Position.x;
            projectile.Y = (int)Position.y;
            projectile.Weapon = _weapon;
            projectile.StartTick = _startTick;
            projectile.VelX = (int) (_direction.x * 100f);
            projectile.VelY = (int) (_direction.y * 100f);
        }
    }
}