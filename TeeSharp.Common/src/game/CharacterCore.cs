using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Common.Game
{
    public class CharacterCore : BaseCharacterCore
    {
        public CharacterCore()
        {
            QuantizeCore = new SnapshotCharacter();
        }

        public override void Init(WorldCore worldCore, BaseMapCollision mapCollision)
        {
            World = worldCore;
            MapCollision = mapCollision;
        }

        public override void Reset()
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            HookPosition = Vector2.Zero;
            HookDirection = Vector2.Zero;
            HookTick = 0;
            HookState = HookState.Idle;
            HookedPlayer = -1;
            Jumped = 0;
            Direction = 0;
            Angle = 0;
            TriggeredEvents = CoreEvents.None;

            // TODO reset input
        }

        public override void Tick(SnapshotPlayerInput input)
        {
            TriggeredEvents = CoreEvents.None;

            var isGrounded = false;
            if (MapCollision.IsTileSolid(Position.x + TeeSize / 2, Position.y + TeeSize / 2 + 5))
                isGrounded = true;
            else if (MapCollision.IsTileSolid(Position.x - TeeSize / 2, Position.y + TeeSize / 2 + 5))
                isGrounded = true;

            var vel = Velocity;
            vel.y += World.Tuning["gravity"];

            if (input != null)
            {
                Direction = input.Direction;
                Angle = (int) (MathHelper.Angle(new Vector2(input.TargetX, input.TargetY)) * 256f);

                if (input.IsJump)
                {
                    if ((Jumped & 1) == 0)
                    {
                        if (isGrounded)
                        {
                            TriggeredEvents |= CoreEvents.GroundJump;
                            vel.y = -World.Tuning["ground_jump_impulse"];
                            Jumped |= 1;
                        }
                        else if ((Jumped & 2) == 0)
                        {
                            TriggeredEvents |= CoreEvents.AirJump;
                            vel.y = -World.Tuning["air_jump_impulse"];
                            Jumped |= 3;
                        }
                    }
                }
                else Jumped &= ~1;

                if (input.IsHook)
                {
                    if (HookState == HookState.Idle)
                    {
                        var targetDirection = new Vector2(input.TargetX, input.TargetY).Normalized;
                        HookState = HookState.Flying;
                        HookPosition = Position + targetDirection * TeeSize * 1.5f;
                        HookDirection = targetDirection;
                        HookedPlayer = -1;
                        HookTick = 0;
                        //TriggeredEvents |= CoreEvents.HookLaunch;
                    }
                }
                else
                {
                    HookedPlayer = -1;
                    HookState = HookState.Idle;
                    HookPosition = Position;
                }
            }

            float maxSpeed = isGrounded ? World.Tuning["ground_control_speed"] : World.Tuning["air_control_speed"];
            float accel = isGrounded ? World.Tuning["ground_control_accel"] : World.Tuning["air_control_accel"];
            float friction = isGrounded ? World.Tuning["ground_friction"] : World.Tuning["air_friction"];

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
                //TriggeredEvents |= CoreEvents.HOOK_RETRACT;
            }
            else if (HookState == HookState.Flying)
            {
                var newHookPos = HookPosition + HookDirection * World.Tuning["hook_fire_speed"];
                if (MathHelper.Distance(Position, newHookPos) > World.Tuning["hook_length"])
                {
                    HookState = HookState.RetractStart;
                    newHookPos = Position + (newHookPos - Position).Normalized * World.Tuning["hook_length"];
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

                if (World.Tuning["player_hooking"] > 0)
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
                                TriggeredEvents |= CoreEvents.HookAttachPlayer;
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
                        TriggeredEvents |= CoreEvents.HookAttachGround;
                        HookState = HookState.Grabbed;
                    }
                    else if (goingToRetract)
                    {
                        TriggeredEvents |= CoreEvents.HookHitNoHook;
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
                    var hookVel = (HookPosition - Position).Normalized * World.Tuning["hook_drag_accel"];
                    if (hookVel.y > 0)
                        hookVel.y *= 0.3f;

                    if (hookVel.x < 0 && Direction < 0 || hookVel.x > 0 && Direction > 0)
                        hookVel.x *= 0.95f;
                    else
                        hookVel.x *= 0.75f;

                    var newVel = vel + hookVel;
                    if (newVel.Length < World.Tuning["hook_drag_speed"] || newVel.Length < vel.Length)
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

            if (World.Tuning["player_collision"] > 0 ||
                World.Tuning["player_hooking"] > 0)
            {
                for (var i = 0; i < World.CharacterCores.Length; i++)
                {
                    var characterCore = World.CharacterCores[i];
                    if (characterCore == null || characterCore == this)
                        continue;

                    var distance = MathHelper.Distance(Position, characterCore.Position);
                    var direction = (Position - characterCore.Position).Normalized;

                    if (World.Tuning["player_collision"] > 0 &&
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

                    if (World.Tuning["player_hooking"] > 0 &&
                        HookedPlayer == i)
                    {
                        if (distance > TeeSize * 1.50f)
                        {
                            var hookAccelerate = World.Tuning["hook_drag_accel"] *
                                                 (distance / World.Tuning["hook_length"]);
                            float dragSpeed = World.Tuning["hook_drag_speed"];

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

        public virtual void Fill(CharacterCore from)
        {
            Position = from.Position;
            Velocity = from.Velocity;
            HookPosition = from.HookPosition;
            HookDirection = from.HookDirection;
            HookTick = from.HookTick;
            HookState = from.HookState;
            Jumped = from.Jumped;
            Direction = from.Direction;
            Angle = from.Angle;
            HookedPlayer = from.HookedPlayer;
            TriggeredEvents = from.TriggeredEvents;
        }

        public override void Move()
        {
            var vel = Velocity;
            var rampValue = MathHelper.VelocityRamp(vel.Length * 50,
                World.Tuning["velramp_start"],
                World.Tuning["velramp_range"],
                World.Tuning["velramp_curvature"]);

            vel.x *= rampValue;

            var newPos = Position;
            MapCollision.MoveBox(ref newPos, ref vel, new Vector2(TeeSize, TeeSize), 0);

            vel.x = vel.x * (1.0f / rampValue);

            if (World.Tuning["player_collision"] > 0)
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

        public override void Quantize()
        {
            Write(QuantizeCore);
            Read(QuantizeCore);
        }

        public override void Write(SnapshotCharacterCore core)
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

        public override void Read(SnapshotCharacter core)
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