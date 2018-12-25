//using TeeSharp.Common;
//using TeeSharp.Common.Enums;
//using TeeSharp.Common.Protocol;

//namespace TeeSharp.Server.Game.Entities
//{
//    public class Projectile : Entity<Projectile>
//    {
//        public override float ProximityRadius { get; protected set; }

//        private readonly Weapon _weapon;
//        private readonly int _ownerId;
//        private readonly Vector2 _dir;
//        private readonly int _damage;
//        private readonly bool _explosive;
//        private readonly float _force;
//        private readonly Sound _soundImpact;
//        private int _startTick;
//        private int _lifeSpan;

//        public Projectile(Weapon weapon, int ownerId, Vector2 pos, Vector2 dir,
//            int lifeSpan, int damage, bool explosive, float force, Sound soundImpact) : base(1)
//        {
//            Position = pos;
//            _weapon = weapon;
//            _ownerId = ownerId;
//            _dir = dir;
//            _lifeSpan = lifeSpan;
//            _damage = damage;
//            _explosive = explosive;
//            _force = force;
//            _soundImpact = soundImpact;
//            _startTick = Server.Tick;
//        }

//        public override void Reset()
//        {
//            Destroy();
//        }

//        protected Vector2 GetPos(float t)
//        {
//            var curvature = 0f;
//            var speed = 0f;

//            switch (_weapon)
//            {
//                case Weapon.Gun:
//                    curvature = Tuning["GunCurvature"];
//                    speed = Tuning["GunSpeed"];
//                    break;

//                case Weapon.Shotgun:
//                    curvature = Tuning["ShotgunCurvature"];
//                    speed = Tuning["ShotgunSpeed"];
//                    break;

//                case Weapon.Grenade:
//                    curvature = Tuning["GrenadeCurvature"];
//                    speed = Tuning["GrenadeSpeed"];
//                    break;
//            }

//            return MathHelper.CalcPos(Position, _dir, curvature, speed, t);
//        }

//        public override void Tick()
//        {
//            var prevTime = (Server.Tick - _startTick - 1) / (float) Server.TickSpeed;
//            var currentTime = (Server.Tick - _startTick) / (float) Server.TickSpeed;
//            var prevPos = GetPos(prevTime);
//            var currentPos = GetPos(currentTime);

//            var collide = GameContext.MapCollision.IntersectLine(prevPos, currentPos, out currentPos, out _);
//            var ownerCharacter = GameContext.Players[_ownerId]?.GetCharacter();
//            var targetCharacter = GameWorld.IntersectCharacter(prevPos, currentPos, 6f, ref currentPos, ownerCharacter);

//            _lifeSpan--;

//            if (targetCharacter != null || collide != CollisionFlags.NONE ||
//                _lifeSpan < 0 || GameLayerClipped(currentPos))
//            {
//                if (_lifeSpan >= 0 || _weapon == Weapon.Grenade)
//                    GameContext.CreateSound(currentPos, _soundImpact);

//                if (_explosive)
//                    GameContext.CreateExplosion(currentPos, _ownerId, _weapon, false);
//                else
//                {
//                    targetCharacter?.TakeDamage(_dir * System.Math.Max(0.001f, _force), _damage, _ownerId, _weapon);
//                }

//                Destroy();
//            }
//        }

//        public override void TickPaused()
//        {
//            _startTick++;
//        }

//        public void FillInfo(SnapshotProjectile projectile)
//        {
//            projectile.Position = Position;
//            projectile.Velocity = _dir * 100f;
//            projectile.StartTick = _startTick;
//            projectile.Weapon = _weapon;
//        }

//        public override void OnSnapshot(int snappingClient)
//        {
//            var currentTime = (Server.Tick - _startTick) / (float) Server.TickSpeed;
//            if (NetworkClipped(snappingClient, GetPos(currentTime)))
//                return;

//            var projectile = Server.SnapshotItem<SnapshotProjectile>(IDs[0]);
//            if (projectile == null)
//                return;

//            FillInfo(projectile);
//        }
//    }
//}