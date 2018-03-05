using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Common.Game
{
    public class CharacterCore
    {
        public virtual Vec2 Position { get; set; }
        public virtual Vec2 Velocity { get; set; }
        public virtual Vec2 HookPosition { get; set; }
        public virtual Vec2 HookDirection { get; set; }
        public virtual int HookTick { get; set; }
        public virtual HookState HookState { get; set; }
        public virtual int Jumped { get; set; }
        public virtual int Direction { get; set; }
        public virtual int Angle { get; set; }
        public virtual int HookedPlayer { get; set; }
        public virtual CoreEvents TriggeredEvents { get; set; }
        public virtual SnapObj_PlayerInput Input { get; set; }

        protected virtual WorldCore World { get; set; }
        protected virtual BaseCollision Collision { get; set; }
        protected virtual SnapObj_Character QuantizeCore { get; set; } 

        protected const float TEE_SIZE = 28.0f;

        public CharacterCore()
        {
            QuantizeCore = new SnapObj_Character();
            Input = new SnapObj_PlayerInput();
        }

        public virtual void Init(WorldCore worldCore, BaseCollision collision)
        {
            World = worldCore;
            Collision = collision;
        }

        public virtual void Reset()
        {
            Position = Vec2.zero;
            Velocity = Vec2.zero;
            HookPosition = Vec2.zero;
            HookDirection = Vec2.zero;
            HookTick = 0;
            HookState =  HookState.IDLE;
            HookedPlayer = -1;
            Jumped = 0;
            Direction = 0;
            Angle = 0;
            TriggeredEvents = CoreEvents.NONE;

            Input.Reset();
        }

        public virtual void Tick(bool useInput)
        {
            TriggeredEvents = CoreEvents.NONE;

            var isGrounded = false;
            if (Collision.IsTileSolid(Position.x + TEE_SIZE / 2, Position.y + TEE_SIZE / 2 + 5))
                isGrounded = true;
            else if (Collision.IsTileSolid(Position.x - TEE_SIZE / 2, Position.y + TEE_SIZE / 2 + 5))
                isGrounded = true;

            var targetDirection = new Vec2(Input.TargetX, Input.TargetY).Normalized;
            float maxSpeed = isGrounded ? World.Tuning["GroundControlSpeed"] : World.Tuning["AirControlSpeed"];
            float accel = isGrounded ? World.Tuning["GroundControlAccel"] : World.Tuning["AirControlAccel"];
            float friction = isGrounded ? World.Tuning["GroundFriction"] : World.Tuning["AirFriction"];

            var vel = Velocity;
            vel.y += World.Tuning["Gravity"];

            if (useInput)
            {
                Direction = Input.Direction;
                var angle = 0d;
                angle = Input.TargetX == 0 
                    ? System.Math.Atan(Input.TargetY) 
                    : System.Math.Atan(Input.TargetY / (float) Input.TargetX);

                if (Input.TargetX < 0)
                    angle += System.Math.PI;
                Angle = (int) (angle * 256.0f);

                if (Input.Jump)
                {
                    if ((Jumped & 1) == 0)
                    {
                        if (isGrounded)
                        {
                            TriggeredEvents |= CoreEvents.GROUND_JUMP;
                            vel.y = -World.Tuning["GroundJumpImpulse"];
                            Jumped |= 1;
                        }
                        else if ((Jumped & 2) == 0)
                        {
                            TriggeredEvents |= CoreEvents.AIR_JUMP;
                            vel.y = -World.Tuning["AirJumpImpulse"];
                            Jumped |= 3;
                        }
                    }
                }
                else Jumped &= ~1;

                if (Input.Hook)
                {
                    if (HookState == HookState.IDLE)
                    {
                        HookState = HookState.FLYING;
                        HookPosition = Position + targetDirection * TEE_SIZE * 1.5f;
                        HookDirection = targetDirection;
                        HookedPlayer = -1;
                        HookTick = 0;
                        TriggeredEvents |= CoreEvents.HOOK_LAUNCH;
                    }
                }
                else
                {
                    HookedPlayer = -1;
                    HookState = HookState.IDLE;
                    HookPosition = Position;
                }
            }

            if (Direction < 0)
                vel.x = Math.SaturatedAdd(-maxSpeed, maxSpeed, vel.x, -accel);
            else if (Direction > 0)
                vel.x = Math.SaturatedAdd(-maxSpeed, maxSpeed, vel.x, +accel);
            else
                vel.x *= friction;

            if (isGrounded)
                Jumped &= ~2;

            if (HookState == HookState.IDLE)
            {
                HookedPlayer = -1;
                HookPosition = Position;
            }
            else if (HookState >= HookState.RETRACT_START && HookState < HookState.RETRACT_END)
            {
                HookState++;
            }
            else if (HookState == HookState.RETRACT_END)
            {
                HookState = HookState.RETRACTED;
                TriggeredEvents |= CoreEvents.HOOK_RETRACT;
            }
            else if (HookState == HookState.FLYING)
            {
                var newHookPos = HookPosition + HookDirection * World.Tuning["HookFireSpeed"];
                if (Math.Distance(Position, newHookPos) > World.Tuning["HookLength"])
                {
                    HookState = HookState.RETRACT_START;
                    newHookPos = Position + (newHookPos - Position).Normalized * World.Tuning["HookLength"];
                }

                var goingToHitGround = false;
                var goingToRetract = false;
                var hitFlags = Collision.IntersectLine(HookPosition, newHookPos, 
                    out newHookPos, out var _);

                if (hitFlags != TileFlags.NONE)
                {
                    if (hitFlags.HasFlag(TileFlags.NOHOOK))
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

                        var closestPoint = Math.ClosestPointOnLine(HookPosition, newHookPos,
                            characterCore.Position);
                        if (Math.Distance(characterCore.Position, closestPoint) < TEE_SIZE + 2f)
                        {
                            if (HookedPlayer == -1 || Math.Distance(HookPosition, characterCore.Position) < distance)
                            {
                                TriggeredEvents |= CoreEvents.HOOK_ATTACH_PLAYER;
                                HookState = HookState.GRABBED;
                                HookedPlayer = i;
                                distance = Math.Distance(HookPosition, characterCore.Position);
                                break;
                            }  
                        }
                    }
                }

                if (HookState == HookState.FLYING)
                {
                    if (goingToHitGround)
                    {
                        TriggeredEvents |= CoreEvents.HOOK_ATTACH_GROUND;
                        HookState = HookState.GRABBED;
                    }
                    else if (goingToRetract)
                    {
                        TriggeredEvents |= CoreEvents.HOOK_HIT_NOHOOK;
                        HookState = HookState.RETRACT_START;
                    }

                    HookPosition = newHookPos;
                }
            }

            if (HookState == HookState.GRABBED)
            {
                if (HookedPlayer != -1)
                {
                    var characterCore = World.CharacterCores[HookedPlayer];
                    if (characterCore != null)
                        HookPosition = characterCore.Position;
                    else
                    {
                        HookedPlayer = -1;
                        HookState = HookState.RETRACTED;
                        HookPosition = Position;
                    }
                }

                if (HookedPlayer == -1 && Math.Distance(HookPosition, Position) > 46.0f)
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
                    HookState = HookState.RETRACTED;
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

                    var distance = Math.Distance(Position, characterCore.Position);
                    var direction = (Position - characterCore.Position).Normalized;

                    if (World.Tuning["PlayerCollision"] > 0 &&
                        distance < TEE_SIZE * 1.25f &&
                        distance > 0)
                    {
                        var a = (TEE_SIZE * 1.45f - distance);
                        var velocity = 0.5f;

                        if (vel.Length > 0.0001)
                            velocity = 1 - (Math.Dot(vel.Normalized, direction) + 1) / 2;

                        vel += direction * a * (velocity * 0.75f);
                        vel *= 0.85f;
                    }

                    if (World.Tuning["PlayerHooking"] > 0 &&
                        HookedPlayer == i)
                    {
                        if (distance > TEE_SIZE * 1.50f)
                        {
                            var hookAccel = World.Tuning["HookDragAccel"] *
                                (distance / World.Tuning["HookLength"]);
                            float dragSpeed = World.Tuning["HookDragSpeed"];

                            characterCore.Velocity = new Vec2(
                                Math.SaturatedAdd(-dragSpeed, dragSpeed, 
                                    characterCore.Velocity.x, hookAccel*direction.x*1.5f),
                                Math.SaturatedAdd(-dragSpeed, dragSpeed, 
                                    characterCore.Velocity.y, hookAccel*direction.y*1.5f)
                            );

                            vel.x = Math.SaturatedAdd(-dragSpeed, dragSpeed, vel.x, 
                                -hookAccel * direction.x * 0.25f);
                            vel.y = Math.SaturatedAdd(-dragSpeed, dragSpeed, vel.y, 
                                -hookAccel * direction.y * 0.25f);
                        }
                    }
                }
            }

            if (vel.Length > 6000)
                vel = vel.Normalized * 6000;
            Velocity = vel;
        }

        public virtual void FillTo(CharacterCore output)
        {
            output.Position = Position;
            output.Velocity = Position;
            output.HookPosition = HookPosition;
            output.HookDirection = HookDirection;
            output.HookTick = HookTick;
            output.HookState = HookState;
            output.Jumped = Jumped;
            output.Direction = Direction;
            output.Angle = Angle;
            output.HookedPlayer = HookedPlayer;
            output.TriggeredEvents = TriggeredEvents;
            output.Input.FillFrom(Input);
        }

        public virtual void Move()
        {
            var vel = Velocity;
            var rampValue = Math.VelocityRamp(vel.Length * 50,
                World.Tuning["VelrampStart"],
                World.Tuning["VelrampRange"],
                World.Tuning["VelrampCurvature"]);

            vel.x *= rampValue;

            var newPos = Position;
            Collision.MoveBox(ref newPos, ref vel, new Vec2(TEE_SIZE, TEE_SIZE), 0);

            vel.x = vel.x * (1.0f / rampValue);

            if (World.Tuning["PlayerCollision"] > 0)
            {
                var distance = Math.Distance(Position, newPos);
                var end = (int) (distance + 1);
                var lastPos = Position;

                for (var i = 0; i < end; i++)
                {
                    var amount = i / distance;
                    var pos = Math.Mix(Position, newPos, amount);

                    for (var c = 0; c < World.CharacterCores.Length; c++)
                    {
                        var character = World.CharacterCores[c];
                        if (character == null || character == this)
                            continue;

                        var d = Math.Distance(pos, character.Position);
                        if (d < TEE_SIZE && d > 0)
                        {
                            if (amount > 0)
                                Position = lastPos;
                            else if (Math.Distance(newPos, character.Position) > d)
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

        public virtual void Write(SnapObj_Character character)
        {
            character.PosX = Math.RoundToInt(Position.x);
            character.PosY = Math.RoundToInt(Position.y);

            character.VelX = Math.RoundToInt(Velocity.x * 256.0f);
            character.VelY = Math.RoundToInt(Velocity.y * 256.0f);

            character.HookState = HookState;
            character.HookTick = HookTick;

            character.HookX = Math.RoundToInt(HookPosition.x);
            character.HookY = Math.RoundToInt(HookPosition.y);

            character.HookDx = Math.RoundToInt(HookDirection.x * 256.0f);
            character.HookDy = Math.RoundToInt(HookDirection.y * 256.0f);

            character.HookedPlayer = HookedPlayer;
            character.Jumped = Jumped;
            character.Direction = Direction;
            character.Angle = Angle;
        }

        public virtual void Read(SnapObj_Character character)
        {
            Position = new Vec2(character.PosX, character.PosY);
            Velocity = new Vec2(character.VelX / 256.0f, character.VelY / 256.0f);

            HookState = character.HookState;
            HookTick = character.HookTick;
            HookPosition = new Vec2(character.HookX, character.HookY);
            HookDirection = new Vec2(character.HookDx / 256.0f, character.HookDy / 256.0f);
            HookedPlayer = character.HookedPlayer;

            Jumped = character.Jumped;
            Direction = character.Direction;
            Angle = character.Angle;
        }
    }
}