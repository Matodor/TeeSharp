using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;

namespace TeeSharp.Server.Game.Entities
{
    public class Projectile : Entity<Projectile>
    {
        public readonly Weapon Weapon;
        public readonly int OwnerId;
        public readonly Vector2 Direction;
        public readonly int Damage;
        public readonly bool Explosive;
        public readonly float Force;
        public readonly Sound SoundImpact;
        private int _startTick;
        private int _lifeSpan;

        public override float ProximityRadius { get; protected set; }

        public Projectile(Weapon weapon, int ownerId, Vector2 startPos, Vector2 direction,
            int lifeSpan, int damage, bool explosive, float force, Sound soundImpact) : base(idsCount: 1)
        {
            Position = startPos;
            Weapon = weapon;
            OwnerId = ownerId;
            Direction = direction;
            Damage = damage;
            Explosive = explosive;
            Force = force;
            SoundImpact = soundImpact;
            _lifeSpan = lifeSpan;
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

            var prevTime = (Server.Tick - _startTick - 1) / (float) Server.TickSpeed;
            var currentTime = (Server.Tick - _startTick) / (float) Server.TickSpeed;
            var prevPos = GetPos(prevTime);
            var currentPos = GetPos(currentTime);

            var collideFlags = GameContext.MapCollision.IntersectLine(prevPos, currentPos, out _, out _);
            var ownerCharacter = GameContext.Players[OwnerId]?.GetCharacter();
            var targetCharacter =
                GameWorld.IntersectCharacter(prevPos, currentPos, 6.0f, ref currentPos, ownerCharacter);

            _lifeSpan--;

            if (_lifeSpan < 0 ||
                targetCharacter != null ||
                collideFlags.HasFlag(CollisionFlags.Solid) ||
                GameLayerClipped(currentPos))
            {
                if (_lifeSpan >= 0 || Weapon == Weapon.Grenade)
                    GameContext.CreateSound(currentPos, SoundImpact);

                if (Explosive)
                    GameContext.CreateExplosion(currentPos, OwnerId, Weapon, Damage);
                else
                {
                    targetCharacter?.TakeDamage(
                        force: Direction * System.Math.Max(0.001f, Force),
                        source: Direction * -1,
                        damage: Damage,
                        from: OwnerId,
                        weapon: Weapon);
                }

                Destroy();
            }
        }

        public override void TickPaused()
        {
            base.TickPaused();

            _startTick++;
        }

        private Vector2 GetPos(float t)
        {
            var curvature = 0f;
            var speed = 0f;

            switch (Weapon)
            {
                case Weapon.Gun:
                    curvature = Tuning["gun_curvature"];
                    speed = Tuning["gun_speed"];
                    break;

                case Weapon.Shotgun:
                    curvature = Tuning["shotgun_curvature"];
                    speed = Tuning["shotgun_speed"];
                    break;

                case Weapon.Grenade:
                    curvature = Tuning["grenade_curvature"];
                    speed = Tuning["grenade_speed"];
                    break;
            }

            return MathHelper.CalcPos(Position, Direction, curvature, speed, t);
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
            projectile.Y = (int) Position.y;
            projectile.Weapon = Weapon;
            projectile.StartTick = _startTick;
            projectile.VelX = (int) (Direction.x * 100f);
            projectile.VelY = (int) (Direction.y * 100f);
        }
    }
}