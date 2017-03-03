using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    class CProjectile : CEntity
    {
        private vec2 m_Direction;
        private int m_LifeSpan;
        private readonly int m_Owner;
        private readonly int m_Type;
        private readonly int m_Damage;
        private readonly int m_SoundImpact;
        private readonly int m_Weapon;
        private readonly float m_Force;
        private int m_StartTick;
        private readonly bool m_Explosive;

        public CProjectile(CGameWorld pGameWorld, int Type, int Owner, vec2 Pos, vec2 Dir, int Span,
            int Damage, bool Explosive, float Force, int SoundImpact, int Weapon)
            : base(pGameWorld, CGameWorld.ENTTYPE_PROJECTILE)
        {
            m_Type = Type;
            m_Pos = Pos;
            m_Direction = Dir;
            m_LifeSpan = Span;
            m_Owner = Owner;
            m_Force = Force;
            m_Damage = Damage;
            m_SoundImpact = SoundImpact;
            m_Weapon = Weapon;
            m_StartTick = Server.Tick();
            m_Explosive = Explosive;

            GameWorld.InsertEntity(this);
        }

        public override void Reset()
        {
            GameWorld.DestroyEntity(this);
        }

        private vec2 GetPos(float Time)
        {
            float Curvature = 0;
            float Speed = 0;

            switch (m_Type)
            {
                case (int)Consts.WEAPON_GRENADE:
                    Curvature = GameServer.Tuning["GrenadeCurvature"];
                    Speed = GameServer.Tuning["GrenadeSpeed"];
                    break;

                case (int)Consts.WEAPON_SHOTGUN:
                    Curvature = GameServer.Tuning["ShotgunCurvature"];
                    Speed = GameServer.Tuning["ShotgunSpeed"];
                    break;

                case (int)Consts.WEAPON_GUN:
                    Curvature = GameServer.Tuning["GunCurvature"];
                    Speed = GameServer.Tuning["GunSpeed"];
                    break;
            }

            return GCHelpers.CalcPos(m_Pos, m_Direction, Curvature, Speed, Time);
        }

        public override void Tick()
        {
            float Pt = (Server.Tick() - m_StartTick - 1) / (float)Server.TickSpeed();
            float Ct = (Server.Tick() - m_StartTick) / (float)Server.TickSpeed();
            vec2 PrevPos = GetPos(Pt);
            vec2 CurPos = GetPos(Ct);
            vec2 forRef;
            int Collide = GameServer.Collision.IntersectLine(PrevPos, CurPos, out CurPos, out forRef);
            CCharacter OwnerChar = GameServer.GetPlayerChar(m_Owner);
            CCharacter TargetChr = GameServer.m_World.IntersectCharacter(PrevPos, CurPos, 6.0f, ref CurPos, OwnerChar);

            m_LifeSpan--;

            if (TargetChr != null || Collide != 0 || m_LifeSpan < 0 || GameLayerClipped(CurPos))
            {
                if (m_LifeSpan >= 0 || m_Weapon == (int)Consts.WEAPON_GRENADE)
                    GameServer.CreateSound(CurPos, m_SoundImpact);

                if (m_Explosive)
                    GameServer.CreateExplosion(CurPos, m_Owner, m_Weapon, false);

                else if (TargetChr != null)
                    TargetChr.TakeDamage(m_Direction * Math.Max(0.001f, m_Force), m_Damage, m_Owner, m_Weapon);

                GameWorld.DestroyEntity(this);
            }
        }

        public override void TickPaused()
        {
            ++m_StartTick;
        }

        public void FillInfo(CNetObj_Projectile pProj)
        {
            pProj.m_X = (int)m_Pos.x;
            pProj.m_Y = (int)m_Pos.y;
            pProj.m_VelX = (int)(m_Direction.x * 100.0f);
            pProj.m_VelY = (int)(m_Direction.y * 100.0f);
            pProj.m_StartTick = m_StartTick;
            pProj.m_Type = m_Type;
        }

        public override void Snap(int SnappingClient)
        {
            //return;

            float Ct = (Server.Tick() - m_StartTick) / (float)Server.TickSpeed();

            if (NetworkClipped(SnappingClient, GetPos(Ct)))
                return;

            CNetObj_Projectile pProj = Server.SnapNetObj<CNetObj_Projectile>((int)Consts.NETOBJTYPE_PROJECTILE, m_IDs[0]);

            if (pProj == null)
                return;

            FillInfo(pProj);
        }
    }
}
