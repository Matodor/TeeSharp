using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Common.Game
{
    public class CharacterCore : BaseCharacterCore
    {
        public CharacterCore()
        {
            QuantizeCore = new SnapshotCharacter();
            Input = new SnapshotPlayerInput();
        }

        public virtual void Init(WorldCore worldCore, BaseMapCollision mapCollision)
        {
            World = worldCore;
            MapCollision = mapCollision;
        }

        public virtual void Reset()
        {
            Position = Vector2.zero;
            Velocity = Vector2.zero;
            HookPosition = Vector2.zero;
            HookDirection = Vector2.zero;
            HookTick = 0;
            HookState = HookState.Idle;
            HookedPlayer = -1;
            Jumped = 0;
            Direction = 0;
            Angle = 0;
            TriggeredEvents = CoreEventFlags.None;

            // TODO reset input
        }

        public virtual void Tick(bool useInput)
        {
            TriggeredEvents = CoreEventFlags.None;

            var isGrounded = false;
            if (MapCollision.IsTileSolid(Position.x + TeeSize / 2, Position.y + TeeSize / 2 + 5))
                isGrounded = true;
            else if (MapCollision.IsTileSolid(Position.x - TeeSize / 2, Position.y + TeeSize / 2 + 5))
                isGrounded = true;

            var targetDirection = new Vector2(Input.TargetX, Input.TargetY).Normalized;
            var vel = Velocity;
            vel.y += World.Tuning["Gravity"];

            if (useInput)
            {
                Direction = Input.Direction;
                Angle = (int) (MathHelper.Angle(new Vector2(Input.TargetX, Input.TargetY)) * 256f);

                if (Input.IsJump)
                {
                    if ((Jumped & 1) == 0)
                    {
                        if (isGrounded)
                        {
                            TriggeredEvents |= CoreEventFlags.GroundJump;
                            vel.y = -World.Tuning["GroundJumpImpulse"];
                            Jumped |= 1;
                        }
                        else if ((Jumped & 2) == 0)
                        {
                            TriggeredEvents |= CoreEventFlags.AirJump;
                            vel.y = -World.Tuning["AirJumpImpulse"];
                            Jumped |= 3;
                        }
                    }
                }
                else Jumped &= ~1;

                if (Input.IsHook)
                {
                    if (HookState == HookState.Idle)
                    {
                        HookState = HookState.Flying;
                        HookPosition = Position + targetDirection * TeeSize * 1.5f;
                        HookDirection = targetDirection;
                        HookedPlayer = -1;
                        HookTick = 0;
                        //TriggeredEvents |= CoreEventFlags.HookLaunch;
                    }
                }
                else
                {
                    HookedPlayer = -1;
                    HookState = HookState.Idle;
                    HookPosition = Position;
                }
            }

            float maxSpeed = isGrounded ? World.Tuning["GroundControlSpeed"] : World.Tuning["AirControlSpeed"];
            float accel = isGrounded ? World.Tuning["GroundControlAccel"] : World.Tuning["AirControlAccel"];
            float friction = isGrounded ? World.Tuning["GroundFriction"] : World.Tuning["AirFriction"];

            if (Direction < 0)
                vel.x = MathHelper.SaturatedAdd(-maxSpeed, maxSpeed, vel.x, -accel);
            else if (Direction > 0)
                vel.x = MathHelper.SaturatedAdd(-maxSpeed, maxSpeed, vel.x, +accel);
            else
                vel.x *= friction;

            if (isGrounded)
                Jumped &= ~2;

            if (HookState == HookState.Idle)
            {
                HookedPlayer = -1;
                HookPosition = Position;
            }
            else if (HookState >= HookState.RetractStart && HookState < HookState.RetractEnd)
            {
                HookState++;
            }
            else if (HookState == HookState.RetractEnd)
            {
                HookState = HookState.Retracted;
                //TriggeredEvents |= CoreEventFlags.HOOK_RETRACT;
            }
            else if (HookState == HookState.Flying)
            {
                var newHookPos = HookPosition + HookDirection * World.Tuning["HookFireSpeed"];
                if (MathHelper.Distance(Position, newHookPos) > World.Tuning["HookLength"])
                {
                    HookState = HookState.RetractStart;
                    newHookPos = Position + (newHookPos - Position).Normalized * World.Tuning["HookLength"];
                }

                var goingToHitGround = false;
                var goingToRetract = false;
                var hitFlags = MapCollision.IntersectLine(HookPosition, newHookPos,
                    out newHookPos, out _);

                if (hitFlags != CollisionFlags.None)
                {
                    if (hitFlags.HasFlag(CollisionFlags.NoHook))
                        goingToRetract = true;
                    else
                        goingToHitGround = true;
                }

                if (World.Tuning["PlayerHooking"] > 0)
                {
                    var distance = 0f;
                    for (var i = 0; i < World.CharacterCores.Length; i++)
                    {
                        var characterCore = World.CharacterCores[i];
                        if (characterCore == null || characterCore == this)
                            continue;

                        var closestPoint = MathHelper.ClosestPointOnLine(HookPosition, newHookPos,
                            characterCore.Position);
                        if (MathHelper.Distance(characterCore.Position, closestPoint) < TeeSize + 2f)
                        {
                            if (HookedPlayer == -1 || MathHelper.Distance(HookPosition, characterCore.Position) < distance)
                            {
                                TriggeredEvents |= CoreEventFlags.HookAttachPlayer;
                                HookState = HookState.Grabbed;
                                HookedPlayer = i;
                                distance = MathHelper.Distance(HookPosition, characterCore.Position);
                                break;
                            }
                        }
                    }
                }

                if (HookState == HookState.Flying)
                {
                    if (goingToHitGround)
                    {
                        TriggeredEvents |= CoreEventFlags.HookAttachGround;
                        HookState = HookState.Grabbed;
                    }
                    else if (goingToRetract)
                    {
                        TriggeredEvents |= CoreEventFlags.HookHitNoHook;
                        HookState = HookState.RetractStart;
                    }

                    HookPosition = newHookPos;
                }
            }

            if (HookState == HookState.Grabbed)
            {
                if (HookedPlayer != -1)
                {
                    var characterCore = World.CharacterCores[HookedPlayer];
                    if (characterCore != null)
                        HookPosition = characterCore.Position;
                    else
                    {
                        HookedPlayer = -1;
                        HookState = HookState.Retracted;
                        HookPosition = Position;
                    }
                }

                if (HookedPlayer == -1 && MathHelper.Distance(HookPosition, Position) > 46.0f)
                {
                    var hookVel = (HookPosition - Position).Normalized * World.Tuning["HookDragAccel"];
                    if (hookVel.y > 0)
                        hookVel.y *= 0.3f;

                    if (hookVel.x < 0 && Direction < 0 || hookVel.x > 0 && Direction > 0)
                        hookVel.x *= 0.95f;
                    else
                        hookVel.x *= 0.75f;

                    var newVel = vel + hookVel;
                    if (newVel.Length < World.Tuning["HookDragSpeed"] || newVel.Length < vel.Length)
                        vel = newVel;
                }

                HookTick++;

                // 60 = 1.25s
                if (HookedPlayer != -1 && (HookTick > 60 || World.CharacterCores[HookedPlayer] == null))
                {
                    HookedPlayer = -1;
                    HookState = HookState.Retracted;
                    HookPosition = Position;
                }
            }

            if (World.Tuning["PlayerCollision"] > 0 ||
                World.Tuning["PlayerHooking"] > 0)
            {
                for (var i = 0; i < World.CharacterCores.Length; i++)
                {
                    var characterCore = World.CharacterCores[i];
                    if (characterCore == null || characterCore == this)
                        continue;

                    var distance = MathHelper.Distance(Position, characterCore.Position);
                    var direction = (Position - characterCore.Position).Normalized;

                    if (World.Tuning["PlayerCollision"] > 0 &&
                        distance < TeeSize * 1.25f &&
                        distance > 0)
                    {
                        var a = (TeeSize * 1.45f - distance);
                        var velocity = 0.5f;

                        if (vel.Length > 0.0001f)
                            velocity = 1 - (MathHelper.Dot(vel.Normalized, direction) + 1) / 2;

                        vel += direction * a * (velocity * 0.75f);
                        vel *= 0.85f;
                    }

                    if (World.Tuning["PlayerHooking"] > 0 &&
                        HookedPlayer == i)
                    {
                        if (distance > TeeSize * 1.50f)
                        {
                            var hookAccelerate = World.Tuning["HookDragAccel"] *
                                                 (distance / World.Tuning["HookLength"]);
                            float dragSpeed = World.Tuning["HookDragSpeed"];

                            characterCore.Velocity = new Vector2(
                                MathHelper.SaturatedAdd(-dragSpeed, dragSpeed,
                                    characterCore.Velocity.x, hookAccelerate * direction.x * 1.5f),
                                MathHelper.SaturatedAdd(-dragSpeed, dragSpeed,
                                    characterCore.Velocity.y, hookAccelerate * direction.y * 1.5f)
                            );

                            vel.x = MathHelper.SaturatedAdd(-dragSpeed, dragSpeed, vel.x,
                                -hookAccelerate * direction.x * 0.25f);
                            vel.y = MathHelper.SaturatedAdd(-dragSpeed, dragSpeed, vel.y,
                                -hookAccelerate * direction.y * 0.25f);
                        }
                    }
                }
            }

            if (vel.Length > 6000)
                vel = vel.Normalized * 6000;
            Velocity = vel;
        }

        //public virtual void FillTo(CharacterCore output)
        //{
        //    output.Position = Position;
        //    output.Velocity = Velocity;
        //    output.HookPosition = HookPosition;
        //    output.HookDirection = HookDirection;
        //    output.HookTick = HookTick;
        //    output.HookState = HookState;
        //    output.Jumped = Jumped;
        //    output.Direction = Direction;
        //    output.Angle = Angle;
        //    output.HookedPlayer = HookedPlayer;
        //    output.TriggeredEvents = TriggeredEvents;
        //    output.Input.FillFrom(Input);
        //}

        public virtual void Move()
        {
            var vel = Velocity;
            var rampValue = MathHelper.VelocityRamp(vel.Length * 50,
                World.Tuning["VelrampStart"],
                World.Tuning["VelrampRange"],
                World.Tuning["VelrampCurvature"]);

            vel.x *= rampValue;

            var newPos = Position;
            MapCollision.MoveBox(ref newPos, ref vel, new Vector2(TeeSize, TeeSize), 0);

            vel.x = vel.x * (1.0f / rampValue);

            if (World.Tuning["PlayerCollision"] > 0)
            {
                var distance = MathHelper.Distance(Position, newPos);
                var end = (int) (distance + 1);
                var lastPos = Position;

                for (var i = 0; i < end; i++)
                {
                    var amount = i / distance;
                    var pos = MathHelper.Mix(Position, newPos, amount);

                    for (var c = 0; c < World.CharacterCores.Length; c++)
                    {
                        var character = World.CharacterCores[c];
                        if (character == null || character == this)
                            continue;

                        var d = MathHelper.Distance(pos, character.Position);
                        if (d < TeeSize && d > 0)
                        {
                            if (amount > 0)
                                Position = lastPos;
                            else if (MathHelper.Distance(newPos, character.Position) > d)
                                Position = newPos;

                            return;
                        }
                    }

                    lastPos = pos;
                }
            }

            Position = newPos;
            Velocity = vel;
        }

        public virtual void Quantize()
        {
            Write(QuantizeCore);
            Read(QuantizeCore);
        }

        public virtual void Write(SnapshotCharacter core)
        {
            core.X = MathHelper.RoundToInt(Position.x);
            core.Y = MathHelper.RoundToInt(Position.y);

            core.VelX = MathHelper.RoundToInt(Velocity.x * 256.0f);
            core.VelY = MathHelper.RoundToInt(Velocity.y * 256.0f);

            core.HookState = HookState;
            core.HookTick = HookTick;

            core.HookX = MathHelper.RoundToInt(HookPosition.x);
            core.HookY = MathHelper.RoundToInt(HookPosition.y);

            core.HookDx = MathHelper.RoundToInt(HookDirection.x * 256.0f);
            core.HookDy = MathHelper.RoundToInt(HookDirection.y * 256.0f);

            core.HookedPlayer = HookedPlayer;
            core.Jumped = Jumped;
            core.Direction = Direction;
            core.Angle = Angle;
        }

        public virtual void Read(SnapshotCharacter core)
        {
            Position = new Vector2(core.X, core.Y);
            Velocity = new Vector2(core.VelX / 256.0f, core.VelY / 256.0f);

            HookState = core.HookState;
            HookTick = core.HookTick;
            HookPosition = new Vector2(core.HookX, core.HookY);
            HookDirection = new Vector2(core.HookDx / 256.0f, core.HookDy / 256.0f);
            HookedPlayer = core.HookedPlayer;

            Jumped = core.Jumped;
            Direction = core.Direction;
            Angle = core.Angle;
        }
    }
}