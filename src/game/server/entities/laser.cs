using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    public class CLaser : CEntity
    {
        private vec2 m_From;
        private vec2 m_Dir;
        private float m_Energy;
        private int m_Bounces;
        private int m_EvalTick;
        private int m_Owner;

        public CLaser(CGameWorld pGameWorld, vec2 Pos, vec2 Direction, float StartEnergy, int Owner) : 
            base(pGameWorld, CGameWorld.ENTTYPE_LASER)
        {
            m_Pos = Pos;
            m_Owner = Owner;
            m_Energy = StartEnergy;
            m_Dir = Direction;
            m_Bounces = 0;
            m_EvalTick = 0;
            GameWorld.InsertEntity(this);
            DoBounce();
        }

        public bool HitCharacter(vec2 From, vec2 To)
        {
            vec2 At = new vec2(0, 0);
            CCharacter pOwnerChar = GameServer.GetPlayerChar(m_Owner);
            CCharacter pHit = GameServer.m_World.IntersectCharacter(m_Pos, To, 0.0f, ref At, pOwnerChar);
            if (pHit == null)
                return false;

            m_From = From;
            m_Pos = At;
            m_Energy = -1;
            pHit.TakeDamage(new vec2(0.0f, 0.0f), (int)GameServer.Tuning["LaserDamage"], m_Owner, (int)Consts.WEAPON_RIFLE);
            return true;
        }

        private void DoBounce()
        {
            m_EvalTick = Server.Tick();

            if (m_Energy < 0)
            {
                GameWorld.DestroyEntity(this);
                return;
            }

            vec2 To = m_Pos + m_Dir * m_Energy;
            vec2 outPtr;

            if (GameServer.Collision.IntersectLine(m_Pos, To, out outPtr, out To) != 0)
            {
                if (!HitCharacter(m_Pos, To))
                {
                    // intersected
                    m_From = m_Pos;
                    m_Pos = To;

                    vec2 TempPos = m_Pos;
                    vec2 TempDir = m_Dir * 4.0f;
                    int pBounces = 0;

                    GameServer.Collision.MovePoint(ref TempPos, ref TempDir, 1.0f, ref pBounces);
                    m_Pos = TempPos;
                    m_Dir = VMath.normalize(TempDir);

                    m_Energy -= VMath.distance(m_From, m_Pos) + GameServer.Tuning["LaserBounceCost"];
                    m_Bounces++;

                    if (m_Bounces > GameServer.Tuning["LaserBounceNum"])
                        m_Energy = -1;

                    GameServer.CreateSound(m_Pos, (int)Consts.SOUND_RIFLE_BOUNCE);
                }
            }
            else
            {
                if (!HitCharacter(m_Pos, To))
                {
                    m_From = m_Pos;
                    m_Pos = To;
                    m_Energy = -1;
                }
            }
        }

        public override void Reset()
        {
            GameWorld.DestroyEntity(this);
        }

        public override void Tick()
        {
            if (Server.Tick() > m_EvalTick + (Server.TickSpeed() * GameServer.Tuning["LaserBounceDelay"]) / 1000.0f)
                DoBounce();
        }

        public override void TickPaused()
        {
            ++m_EvalTick;
        }

        public override void Snap(int SnappingClient)
        {
            if (NetworkClipped(SnappingClient))
                return;

            CNetObj_Laser pObj = Server.SnapNetObj<CNetObj_Laser>((int)Consts.NETOBJTYPE_LASER, m_IDs[0]);
            if (pObj == null)
                return;

            pObj.m_X = (int)m_Pos.x;
            pObj.m_Y = (int)m_Pos.y;
            pObj.m_FromX = (int)m_From.x;
            pObj.m_FromY = (int)m_From.y;
            pObj.m_StartTick = m_EvalTick;
        }
    }
}
