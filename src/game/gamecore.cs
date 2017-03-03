using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    public class CTuneParam
    {
        public string Name { get; }
        public string ScriptName { get; }
        public int IntValue { get { return m_Value; } }
        public float FloatValue { get { return m_Value / 100f; } }

        private int m_Value;

        /*public void Set(int v)
        {
            m_Value = v;
        }*/

        public CTuneParam(string name, string scriptName, float value)
        {
            Name = name;
            ScriptName = scriptName;
            float tmp = value*100;
            m_Value =(int)tmp;
            //Set((int)tmp);
        }
    }

    public partial class CTuningParams
    {
        const float TicksPerSecond = 50.0f;

        public float this[string name]
        {
            get
            {
                if (Params.ContainsKey(name))
                    return Params[name].FloatValue;
                return 0;
            }
        }

        public readonly Dictionary<string, CTuneParam> Params;

        public CTuningParams()
        {
            Params = new Dictionary<string, CTuneParam>();
            foreach (var cTuneParam in default_Tuning)
                Params.Add(cTuneParam.Key, cTuneParam.Value);
        }

        public CTuneParam GetParam(string name)
        {
            if (Params.ContainsKey(name))
                return Params[name];
            return null;
        }

        public int Num()
        {
            return Params.Count;
        }

        /*public bool Set(string pName, float v)
        {
            if (TuningParams.ContainsKey(pName))
            {
                TuningParams[pName].m_Value = (int)(v * 100.0f);
                return true;
            }
            return false;
        }

        public int GetIntValue(string pName)
        {
            if (TuningParams.ContainsKey(pName))
            {
                return TuningParams[pName].m_Value;
            }
            return 0;
        }

        public bool Get(string pName, ref float v)
        {
            if (TuningParams.ContainsKey(pName))
            {
                v = TuningParams[pName].m_Value / 100.0f;
                return true;
            }
            return false;
        }*/
    }

    public static class GCHelpers
    {
        public static float HermiteBasis1(float v)
        {
            return 2 * v * v * v - 3 * v * v + 1;
        }

        public static float VelocityRamp(float Value, float Start, float Range, float Curvature)
        {
            if (Value < Start)
                return 1.0f;
            return 1.0f / (float)Math.Pow(Curvature, (Value - Start) / Range);
        }

        public static vec2 GetDirection(int Angle)
        {
            float a = Angle / 256.0f;
            return new vec2((float)Math.Cos(a), (float)Math.Sin(a));
        }

        public static vec2 GetDir(float Angle)
        {
            return new vec2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
        }

        public static float GetAngle(vec2 Dir)
        {
            if (Dir.x == 0 && Dir.y == 0)
                return 0.0f;
            float a = (float)Math.Atan(Dir.y / Dir.x);
            if (Dir.x < 0)
                a = a + CMath.pi;
            return a;
        }

        public static void StrTo3Ints(string pStr, ref int int1, ref int int2, ref int int3)
        {
            //if (string.IsNullOrEmpty(pStr))
            //    return;

            var utf8Bytes = Encoding.UTF8.GetBytes(pStr);
            var charInts = new int[utf8Bytes.Length];

            for (int i = 0; i < charInts.Length; i++)
            {
                if (utf8Bytes[i] >= 128)
                    charInts[i] = utf8Bytes[i] - 256;
                else
                    charInts[i] = utf8Bytes[i];
            }

            int Index = 0;
            int[] aBuf = new int[4] { 0, 0, 0, 0 };
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int1 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            aBuf[0] = 0; aBuf[1] = 0; aBuf[2] = 0; aBuf[3] = 0;
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int2 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            aBuf[0] = 0; aBuf[1] = 0; aBuf[2] = 0; aBuf[3] = 0;
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int3 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            // null terminate
            int3 &= int.MaxValue;
        }

        public static void StrTo4Ints(string pStr, ref int int1, ref int int2, ref int int3, ref int int4)
        {
            //if (string.IsNullOrEmpty(pStr))
            //    return;

            var utf8Bytes = Encoding.UTF8.GetBytes(pStr);
            var charInts = new int[utf8Bytes.Length];

            for (int i = 0; i < charInts.Length; i++)
            {
                if (utf8Bytes[i] >= 128)
                    charInts[i] = utf8Bytes[i] - 256;
                else
                    charInts[i] = utf8Bytes[i];
            }

            int Index = 0;
            int[] aBuf = new int[4] { 0, 0, 0, 0 };
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int1 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            aBuf[0] = 0; aBuf[1] = 0; aBuf[2] = 0; aBuf[3] = 0;
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int2 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            aBuf[0] = 0; aBuf[1] = 0; aBuf[2] = 0; aBuf[3] = 0;
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int3 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            aBuf[0] = 0; aBuf[1] = 0; aBuf[2] = 0; aBuf[3] = 0;
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int4 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            // null terminate
            //int4 &= 0xffffff00;
        }

        public static void StrTo6Ints(string pStr, ref int int1, ref int int2, ref int int3,
            ref int int4, ref int int5, ref int int6)
        {
            //if (string.IsNullOrEmpty(pStr))
            //    return;

            var utf8Bytes = Encoding.UTF8.GetBytes(pStr);
            var charInts = new int[utf8Bytes.Length];

            for (int i = 0; i < charInts.Length; i++)
            {
                if (utf8Bytes[i] >= 128)
                    charInts[i] = utf8Bytes[i] - 256;
                else
                    charInts[i] = utf8Bytes[i];
            }

            int Index = 0;
            int[] aBuf = new int[4] { 0, 0, 0, 0 };
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int1 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            aBuf[0] = 0; aBuf[1] = 0; aBuf[2] = 0; aBuf[3] = 0;
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int2 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            aBuf[0] = 0; aBuf[1] = 0; aBuf[2] = 0; aBuf[3] = 0;
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int3 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            aBuf[0] = 0; aBuf[1] = 0; aBuf[2] = 0; aBuf[3] = 0;
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int4 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            aBuf[0] = 0; aBuf[1] = 0; aBuf[2] = 0; aBuf[3] = 0;
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int5 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);

            aBuf[0] = 0; aBuf[1] = 0; aBuf[2] = 0; aBuf[3] = 0;
            for (int c = 0; c < 4 && Index < charInts.Length; c++, Index++)
                aBuf[c] = charInts[Index];
            int6 = ((aBuf[0] + 128) << 24) | ((aBuf[1] + 128) << 16) | ((aBuf[2] + 128) << 8) | (aBuf[3] + 128);
            // null terminate
            //int4 &= 0xffffff00;
        }

        public static vec2 CalcPos(vec2 Pos, vec2 Velocity, float Curvature, float Speed, float Time)
        {
            vec2 n = new vec2();
            Time *= Speed;
            n.x = Pos.x + Velocity.x * Time;
            n.y = Pos.y + Velocity.y * Time + Curvature / 10000 * (Time * Time);
            return n;
        }

        public static float SaturatedAdd(float Min, float Max, float Current, float Modifier)
        {
            if (Modifier < 0)
            {
                if (Current < Min)
                    return Current;
                Current += Modifier;
                if (Current < Min)
                    Current = Min;
                return Current;
            }
            
            if (Current > Max)
                return Current;
            Current += Modifier;
            if (Current > Max)
                Current = Max;
            return Current;
        }
    }

    public enum GCEnum
    {
        HOOK_RETRACTED = -1,
        HOOK_IDLE = 0,
        HOOK_RETRACT_START = 1,
        HOOK_RETRACT_END = 3,
        HOOK_FLYING,
        HOOK_GRABBED,

        COREEVENT_GROUND_JUMP = 0x01,
        COREEVENT_AIR_JUMP = 0x02,
        COREEVENT_HOOK_LAUNCH = 0x04,
        COREEVENT_HOOK_ATTACH_PLAYER = 0x08,
        COREEVENT_HOOK_ATTACH_GROUND = 0x10,
        COREEVENT_HOOK_HIT_NOHOOK = 0x20,
        COREEVENT_HOOK_RETRACT = 0x40,
    }

    public class CCharacterCore
    {
        public vec2 m_Pos;
        public vec2 m_Vel;

        public vec2 m_HookPos;
        public vec2 m_HookDir;
        public int m_HookTick;
        public int m_HookState;
        public int m_HookedPlayer;

        public int m_Jumped;

        public int m_Direction;
        public int m_Angle;
        public bool m_pReset;
        public CNetObj_PlayerInput m_Input = new CNetObj_PlayerInput();

        public int m_TriggeredEvents;

        private const float PhysSize = 28.0f;
        private CGameContext m_pGameServer;
        private CCollision m_pCollision;
        private CGameWorld m_World;

        public void Write(CCharacterCore output)
        {
            output.m_Pos = m_Pos;
            output.m_Vel = m_Vel;
            output.m_HookPos = m_HookPos;
            output.m_HookDir = m_HookDir;
            output.m_HookTick = m_HookTick;
            output.m_Angle = m_Angle;
            output.m_Direction = m_Direction;
            output.m_HookState = m_HookState;
            output.m_HookedPlayer = m_HookedPlayer;
            output.m_Jumped = m_Jumped;
            output.m_TriggeredEvents = m_TriggeredEvents;
            m_Input.Write(output.m_Input);
        }

        public void Init(CGameWorld world, CGameContext pGameContext)
        {
            m_World = world;
            m_pGameServer = pGameContext;
            m_pCollision = m_pGameServer.Collision;
        }

        public void Reset()
        {
            m_Pos = new vec2(0, 0);
            m_Vel = new vec2(0, 0);
            m_HookPos = new vec2(0, 0);
            m_HookDir = new vec2(0, 0);
            m_HookTick = 0;
            m_HookState = (int)GCEnum.HOOK_IDLE;
            m_HookedPlayer = -1;
            m_Jumped = 0;
            m_TriggeredEvents = 0;
        }

        public void Tick(bool UseInput)
        {
            m_TriggeredEvents = 0;

            // get ground state
            bool Grounded = m_pCollision.CheckPoint(m_Pos.x + PhysSize / 2, m_Pos.y + PhysSize / 2 + 5) || 
                            m_pCollision.CheckPoint(m_Pos.x - PhysSize / 2, m_Pos.y + PhysSize / 2 + 5);

            vec2 TargetDirection = VMath.normalize(new vec2(m_Input.m_TargetX, m_Input.m_TargetY));

            m_Vel.y += m_pGameServer.Tuning["Gravity"];

            float MaxSpeed = Grounded ? m_pGameServer.Tuning["GroundControlSpeed"] : m_pGameServer.Tuning["AirControlSpeed"];
            float Accel = Grounded ? m_pGameServer.Tuning["GroundControlAccel"] : m_pGameServer.Tuning["AirControlAccel"];
            float Friction = Grounded ? m_pGameServer.Tuning["GroundFriction"] : m_pGameServer.Tuning["AirFriction"];

            // handle input
            if (UseInput)
            {
                // setup angle
                float angle;
                if (m_Input.m_TargetX == 0)
                    angle = (float)Math.Atan((float)m_Input.m_TargetY);
                else
                    angle = (float)Math.Atan(m_Input.m_TargetY / (float)m_Input.m_TargetX);

                if (m_Input.m_TargetX < 0)
                    angle = angle + CMath.pi;

                m_Angle = (int)(angle * 256.0f);

                m_Direction = m_Input.m_Direction;
                
                // handle jump
                if (m_Input.m_Jump != 0)
                {
                    if ((m_Jumped & 1) == 0)
                    {
                        if (Grounded)
                        {
                            float f = m_pGameServer.Tuning["GroundJumpImpulse"];
                            m_TriggeredEvents |= (int)GCEnum.COREEVENT_GROUND_JUMP;
                            m_Vel.y = -m_pGameServer.Tuning["GroundJumpImpulse"];
                            m_Jumped |= 1;
                        }
                        else if ((m_Jumped & 2) == 0)
                        {
                            m_TriggeredEvents |= (int)GCEnum.COREEVENT_AIR_JUMP;
                            m_Vel.y = -m_pGameServer.Tuning["AirJumpImpulse"];
                            m_Jumped |= 3;
                        }
                    }
                }
                else
                    m_Jumped &= ~1;

                // handle hook
                if (m_Input.m_Hook != 0)
                {
                    if (m_HookState == (int)GCEnum.HOOK_IDLE)
                    {
                        m_HookState = (int)GCEnum.HOOK_FLYING;
                        m_HookPos = m_Pos + TargetDirection * PhysSize * 1.5f;
                        m_HookDir = TargetDirection;
                        m_HookedPlayer = -1;
                        m_HookTick = 0;
                        m_TriggeredEvents |= (int)GCEnum.COREEVENT_HOOK_LAUNCH;
                    }
                }
                else
                {
                    m_HookedPlayer = -1;
                    m_HookState = (int)GCEnum.HOOK_IDLE;
                    m_HookPos = m_Pos;
                }
            }

            // add the speed modification according to players wanted direction
            if (m_Direction < 0)
                m_Vel.x = GCHelpers.SaturatedAdd(-MaxSpeed, MaxSpeed, m_Vel.x, -Accel);
            if (m_Direction > 0)
                m_Vel.x = GCHelpers.SaturatedAdd(-MaxSpeed, MaxSpeed, m_Vel.x, Accel);
            if (m_Direction == 0)
                m_Vel.x *= Friction;

            // handle jumping
            // 1 bit = to keep track if a jump has been made on this input
            // 2 bit = to keep track if a air-jump has been made
            if (Grounded)
                m_Jumped &= ~2;

            // do hook
            if (m_HookState == (int)GCEnum.HOOK_IDLE)
            {
                m_HookedPlayer = -1;
                m_HookState = (int)GCEnum.HOOK_IDLE;
                m_HookPos = m_Pos;
            }
            else if (m_HookState >= (int)GCEnum.HOOK_RETRACT_START && m_HookState < (int)GCEnum.HOOK_RETRACT_END)
            {
                m_HookState++;
            }
            else if (m_HookState == (int)GCEnum.HOOK_RETRACT_END)
            {
                m_HookState = (int)GCEnum.HOOK_RETRACTED;
                m_TriggeredEvents |= (int)GCEnum.COREEVENT_HOOK_RETRACT;
                m_HookState = (int)GCEnum.HOOK_RETRACTED;
            }
            else if (m_HookState == (int)GCEnum.HOOK_FLYING)
            {
                vec2 NewPos = m_HookPos + m_HookDir * m_pGameServer.Tuning["HookFireSpeed"];
                if (VMath.distance(m_Pos, NewPos) > m_pGameServer.Tuning["HookLength"])
                {
                    m_HookState = (int)GCEnum.HOOK_RETRACT_START;
                    NewPos = m_Pos + VMath.normalize(NewPos - m_Pos) * m_pGameServer.Tuning["HookLength"];
                    m_pReset = true;
                }

                // make sure that the hook doesn't go though the ground
                bool GoingToHitGround = false;
                bool GoingToRetract = false;
                vec2 forRef;
                int Hit = m_pCollision.IntersectLine(m_HookPos, NewPos, out NewPos, out forRef);
                if (Hit != 0)
                {
                    if ((Hit & CCollision.COLFLAG_NOHOOK) != 0)
                        GoingToRetract = true;
                    else
                        GoingToHitGround = true;
                    m_pReset = true;
                }

                // Check against other players first
                if (m_World != null && (int)m_pGameServer.Tuning["PlayerHooking"] != 0)
                {
                    float Distance = 0.0f;
                    for (int i = 0; i < (int)Consts.MAX_CLIENTS; i++)
                    {
                        CCharacterCore pCharCore = m_World.m_apCharacters[i];
                        if (pCharCore == null || pCharCore == this)
                            continue;

                        vec2 ClosestPoint = VMath.closest_point_on_line(m_HookPos, NewPos, pCharCore.m_Pos);
                        if (VMath.distance(pCharCore.m_Pos, ClosestPoint) < PhysSize + 2.0f)
                        {
                            if (m_HookedPlayer == -1 || VMath.distance(m_HookPos, pCharCore.m_Pos) < Distance)
                            {
                                m_TriggeredEvents |= (int) GCEnum.COREEVENT_HOOK_ATTACH_PLAYER;
                                m_HookState = (int) GCEnum.HOOK_GRABBED;
                                m_HookedPlayer = i;
                                Distance = VMath.distance(m_HookPos, pCharCore.m_Pos);
                            }
                        }
                    }
                }

                if (m_HookState == (int)GCEnum.HOOK_FLYING)
                {
                    // check against ground
                    if (GoingToHitGround)
                    {
                        m_TriggeredEvents |= (int)GCEnum.COREEVENT_HOOK_ATTACH_GROUND;
                        m_HookState = (int)GCEnum.HOOK_GRABBED;
                    }
                    else if (GoingToRetract)
                    {
                        m_TriggeredEvents |= (int)GCEnum.COREEVENT_HOOK_HIT_NOHOOK;
                        m_HookState = (int)GCEnum.HOOK_RETRACT_START;
                    }

                    m_HookPos = NewPos;
                }
            }

            if (m_HookState == (int)GCEnum.HOOK_GRABBED)
            {
                if (m_HookedPlayer != -1)
                {
                    CCharacterCore pCharCore = m_World.m_apCharacters[m_HookedPlayer];
                    if (pCharCore != null)
                        m_HookPos = pCharCore.m_Pos;
                    else
                    {
                        // release hook
                        m_HookedPlayer = -1;
                        m_HookState = (int)GCEnum.HOOK_RETRACTED;
                        m_HookPos = m_Pos;
                    }

                    // keep players hooked for a max of 1.5sec
                    //if(Server()->Tick() > hook_tick+(Server()->TickSpeed()*3)/2)
                    //release_hooked();
                }

                // don't do this hook rutine when we are hook to a player
                if (m_HookedPlayer == -1 && VMath.distance(m_HookPos, m_Pos) > 46.0f)
                {
                     float t = m_pGameServer.Tuning["HookDragAccel"];
                    vec2 HookVel = VMath.normalize(m_HookPos - m_Pos) * m_pGameServer.Tuning["HookDragAccel"];
                    // the hook as more power to drag you up then down.
                    // this makes it easier to get on top of an platform
                    if (HookVel.y > 0)
                        HookVel.y *= 0.3f;

                    // the hook will boost it's power if the player wants to move
                    // in that direction. otherwise it will dampen everything abit
                    if ((HookVel.x < 0 && m_Direction < 0) || (HookVel.x > 0 && m_Direction > 0))
                        HookVel.x *= 0.95f;
                    else
                        HookVel.x *= 0.75f;

                    vec2 NewVel = m_Vel + HookVel;

                    // check if we are under the legal limit for the hook
                    if (VMath.length(NewVel) < m_pGameServer.Tuning["HookDragSpeed"] || VMath.length(NewVel) < VMath.length(m_Vel))
                        m_Vel = NewVel; // no problem. apply

                }

                // release hook (max hook time is 1.25
                m_HookTick++;
                if (m_HookedPlayer != -1 && (m_HookTick > (int)Consts.SERVER_TICK_SPEED + (int)Consts.SERVER_TICK_SPEED / 5 || m_World.m_apCharacters[m_HookedPlayer] == null))
                {
                    m_HookedPlayer = -1;
                    m_HookState = (int)GCEnum.HOOK_RETRACTED;
                    m_HookPos = m_Pos;
                }
            }

            if (m_World != null)
            {
                for (int i = 0; i < (int)Consts.MAX_CLIENTS; i++)
                {
                    CCharacterCore pCharCore = m_World.m_apCharacters[i];
                    if (pCharCore == null)
                        continue;

                    if (pCharCore == this)
                        continue;

                    float Distance = VMath.distance(m_Pos, pCharCore.m_Pos);
                    vec2 Dir = VMath.normalize(m_Pos - pCharCore.m_Pos);
                    if ((int)m_pGameServer.Tuning["PlayerCollision"] != 0 && Distance < PhysSize * 1.25f && Distance > 0.0f)
                    {
                        float a = (PhysSize * 1.45f - Distance);
                        float Velocity = 0.5f;

                        // make sure that we don't add excess force by checking the
                        // direction against the current velocity. if not zero.
                        if (VMath.length(m_Vel) > 0.0001)
                            Velocity = 1 - (VMath.dot(VMath.normalize(m_Vel), Dir) + 1) / 2;

                        m_Vel += Dir * a * (Velocity * 0.75f);
                        m_Vel *= 0.85f;
                    }

                    // handle hook influence
                    if (m_HookedPlayer == i && (int)m_pGameServer.Tuning["PlayerHooking"] != 0)
                    {
                        if (Distance > PhysSize * 1.50f) // TODO: fix tweakable variable
                        {
                            float newAccel = m_pGameServer.Tuning["HookDragAccel"] * (Distance / m_pGameServer.Tuning["HookLength"]);
                            float DragSpeed = m_pGameServer.Tuning["HookDragSpeed"];

                            // add force to the hooked player
                            pCharCore.m_Vel.x = GCHelpers.SaturatedAdd(-DragSpeed, DragSpeed, pCharCore.m_Vel.x,
                                newAccel*Dir.x*1.5f);
                            pCharCore.m_Vel.y = GCHelpers.SaturatedAdd(-DragSpeed, DragSpeed, pCharCore.m_Vel.y,
                                newAccel*Dir.y*1.5f);

                            m_Vel.x = GCHelpers.SaturatedAdd(-DragSpeed, DragSpeed, m_Vel.x, -newAccel * Dir.x * 0.25f);
                            m_Vel.y = GCHelpers.SaturatedAdd(-DragSpeed, DragSpeed, m_Vel.y, -newAccel * Dir.y * 0.25f);
                        }
                    }
                }
            }

            // clamp the velocity to something sane
            if (VMath.length(m_Vel) > 6000)
                m_Vel = VMath.normalize(m_Vel) * 6000;
        }

        public void Move()
        {
            float RampValue = GCHelpers.VelocityRamp(VMath.length(m_Vel) * 50, m_pGameServer.Tuning["VelrampStart"], m_pGameServer.Tuning["VelrampRange"], m_pGameServer.Tuning["VelrampCurvature"]);
            m_Vel.x = m_Vel.x * RampValue;

            vec2 NewPos = m_Pos;
            m_pCollision.MoveBox(ref NewPos, ref m_Vel, new vec2(28.0f, 28.0f), 0);

            m_Vel.x = m_Vel.x * (1.0f / RampValue);

            if (m_pGameServer.Tuning["PlayerCollision"] > 0)
            {
                // check player collision
                float Distance = VMath.distance(m_Pos, NewPos);
                int End = (int)(Distance + 1);
                vec2 LastPos = m_Pos;

                for (int i = 0; i < End; i++)
                {
                    float a = i / Distance;
                    vec2 Pos = VMath.mix(m_Pos, NewPos, a);
                    for (int p = 0; p < (int)Consts.MAX_CLIENTS; p++)
                    {
                        CCharacterCore pCharCore = m_World.m_apCharacters[p];
                        if (/*Dummy || */pCharCore == null || pCharCore == this)
                            continue;
                        float D = VMath.distance(Pos, pCharCore.m_Pos);
                        if (D < 28.0f && D > 0.0f)
                        {
                            if (a > 0.0f)
                                m_Pos = LastPos;
                            else if (VMath.distance(NewPos, pCharCore.m_Pos) > D)
                                m_Pos = NewPos;
                            return;
                        }
                    }
                    LastPos = Pos;
                }
            }

            m_Pos = NewPos;
        }

        void Read(CNetObj_Character pObjCore)
        {
            m_Pos.x = pObjCore.m_X;
            m_Pos.y = pObjCore.m_Y;
            m_Vel.x = pObjCore.m_VelX / 256.0f;
            m_Vel.y = pObjCore.m_VelY / 256.0f;
            m_HookState = pObjCore.m_HookState;
            m_HookTick = pObjCore.m_HookTick;
            m_HookPos.x = pObjCore.m_HookX;
            m_HookPos.y = pObjCore.m_HookY;
            m_HookDir.x = pObjCore.m_HookDx / 256.0f;
            m_HookDir.y = pObjCore.m_HookDy / 256.0f;
            m_HookedPlayer = pObjCore.m_HookedPlayer;
            m_Jumped = pObjCore.m_Jumped;
            m_Direction = pObjCore.m_Direction;
            m_Angle = pObjCore.m_Angle;
        }

        public void Write(CNetObj_Character pObjCore)
        {
            pObjCore.m_X = CMath.round_to_int(m_Pos.x);
            pObjCore.m_Y = CMath.round_to_int(m_Pos.y);

            pObjCore.m_VelX = CMath.round_to_int(m_Vel.x * 256.0f);
            pObjCore.m_VelY = CMath.round_to_int(m_Vel.y * 256.0f);
            pObjCore.m_HookState = m_HookState;
            pObjCore.m_HookTick = m_HookTick;
            pObjCore.m_HookX = CMath.round_to_int(m_HookPos.x);
            pObjCore.m_HookY = CMath.round_to_int(m_HookPos.y);
            pObjCore.m_HookDx = CMath.round_to_int(m_HookDir.x * 256.0f);
            pObjCore.m_HookDy = CMath.round_to_int(m_HookDir.y * 256.0f);
            pObjCore.m_HookedPlayer = m_HookedPlayer;
            pObjCore.m_Jumped = m_Jumped;
            pObjCore.m_Direction = m_Direction;
            pObjCore.m_Angle = m_Angle;
        }

        public void Quantize()
        {
            CNetObj_Character Core = new CNetObj_Character();
            Write(Core);
            Read(Core);
        }
    }
}
