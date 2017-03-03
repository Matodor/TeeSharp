using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    public class CFlag : CEntity
    {
        public int m_Team;
        public int m_AtStand;
        public int m_DropTick;
        public int m_GrabTick;

        public const int ms_PhysSize = 14;
        public CCharacter m_pCarryingCharacter;
        public vec2 m_Vel;
        public vec2 m_StandPos;

        public CFlag(CGameWorld pGameWorld, int Team) : base(pGameWorld, CGameWorld.ENTTYPE_FLAG)
        {
            m_Team = Team;
            m_ProximityRadius = ms_PhysSize;
            m_pCarryingCharacter = null;
            m_GrabTick = 0;
        }

        public override void Reset()
        {
            m_pCarryingCharacter = null;
            m_AtStand = 1;
            m_Pos = m_StandPos;
            m_Vel = new vec2(0, 0);
            m_GrabTick = 0;
        }

        public override void TickPaused()
        {
            ++m_DropTick;
            if (m_GrabTick != 0)
                ++m_GrabTick;
        }

        public override void Snap(int SnappingClient)
        {
            if (NetworkClipped(SnappingClient))
                return;

            CNetObj_Flag pFlag = Server.SnapNetObj<CNetObj_Flag>((int)Consts.NETOBJTYPE_FLAG, m_Team);
            if (pFlag == null)
                return;

            pFlag.m_X = (int)m_Pos.x;
            pFlag.m_Y = (int)m_Pos.y;
            pFlag.m_Team = m_Team;
        }
    }
}
