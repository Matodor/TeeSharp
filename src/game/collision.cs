using System;
using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    public class CCollision
    {
        public const int
            COLFLAG_SOLID = 1,
            COLFLAG_DEATH = 2,
            COLFLAG_NOHOOK = 4;

        public int GetWidth() { return m_Width; }
        public int GetHeight() { return m_Height; }

        private CTile[] m_pTiles;
        private int m_Width;
        private int m_Height;
        private CLayers m_pLayers;
        private readonly CGameContext _gameContext;

        public CCollision(CGameContext gameContext)
        {
            _gameContext = gameContext;
            m_pTiles = null;
            m_Width = 0;
            m_Height = 0;
            m_pLayers = null;
        }

        public void Init(CLayers pLayers)
        {
            m_pLayers = pLayers;
            m_Width = m_pLayers.GameLayer().m_Width;
            m_Height = m_pLayers.GameLayer().m_Height;
            m_pTiles = m_pLayers.Map().GetData<CTile>(m_pLayers.GameLayer().m_Data).ToArray();

            for (int i = 0; i < m_Width * m_Height; i++)
            {

                if (m_pTiles[i].m_Index > 175)
                    continue;

                switch (m_pTiles[i].m_Index)
                {
                    case (int)MapItems.TILE_DEATH:
                        m_pTiles[i].m_Index = COLFLAG_DEATH;
                        break;
                    case (int)MapItems.TILE_SOLID:
                        m_pTiles[i].m_Index = COLFLAG_SOLID;
                        break;
                    case (int)MapItems.TILE_NOHOOK:
                        m_pTiles[i].m_Index = COLFLAG_SOLID | COLFLAG_NOHOOK;
                        break;
                }
            }
        }

        public bool IsTileSolid(int x, int y)
        {
            return (GetTile(x, y) & COLFLAG_SOLID) != 0;
        }

        public CTile GetTileAtPos(int x, int y)
        {
            int Nx = CMath.clamp(x / 32, 0, m_Width - 1);
            int Ny = CMath.clamp(y / 32, 0, m_Height - 1);

            if (m_pTiles == null || Ny < 0 || Nx < 0)
                return null;

            return m_pTiles[Ny * m_Width + Nx];
        }

        public vec2 GetPos(int index)
        {
            if (index < 0)
                return new vec2(0, 0);

            int x = index % m_Width;
            int y = index / m_Width;
            return new vec2(x * 32 + 16, y * 32 + 16);
        }

        public int GetIndex(vec2 pos)
        {
            int Nx = CMath.clamp((int)pos.x / 32, 0, m_Width - 1);
            int Ny = CMath.clamp((int)pos.y / 32, 0, m_Height - 1);
            return Ny*m_Width + Nx;
        }

        public CTile GetTileAtPos(vec2 pos)
        {
            int Nx = CMath.clamp((int)pos.x / 32, 0, m_Width - 1);
            int Ny = CMath.clamp((int)pos.y / 32, 0, m_Height - 1);

            if (m_pTiles == null || Ny < 0 || Nx < 0)
                return null;

            return m_pTiles[Ny * m_Width + Nx];
        }

        private int GetTile(int x, int y)
        {
            int Nx = CMath.clamp(x / 32, 0, m_Width - 1);
            int Ny = CMath.clamp(y / 32, 0, m_Height - 1);

            int index = m_pTiles[Ny*m_Width + Nx].m_Index;
            if (index == COLFLAG_SOLID || index == (COLFLAG_SOLID | COLFLAG_NOHOOK) || index == COLFLAG_DEATH)
                return index;
            return 0;
        }

        public bool IntersectLine(vec2 Pos0, vec2 Pos1)
        {
            float Distance = VMath.distance(Pos0, Pos1);
            int End = (int)(Distance + 1);

            for (int i = 0; i < End; i++)
            {
                float a = i / Distance;
                vec2 Pos = VMath.mix(Pos0, Pos1, a);

                if (CheckPoint(Pos.x, Pos.y))
                    return true;
            }
            return false;
        }

        public int IntersectLine(vec2 Pos0, vec2 Pos1, out vec2 pOutCollision, out vec2 pOutBeforeCollision)
        {
            float Distance = VMath.distance(Pos0, Pos1);
            int End = (int)(Distance + 1);
            vec2 Last = Pos0;

            for (int i = 0; i < End; i++)
            {
                float a = i / Distance;
                vec2 Pos = VMath.mix(Pos0, Pos1, a);
                if (CheckPoint(Pos.x, Pos.y))
                {
                    pOutCollision = Pos;
                    pOutBeforeCollision = Last;
                    return GetCollisionAt(Pos.x, Pos.y);
                }
                Last = Pos;
            }

            pOutCollision = Pos1;
            pOutBeforeCollision = Pos1;
            return 0;
        }

        // TODO: OPT: rewrite this smarter!
        public bool MovePoint(ref vec2 pInoutPos, ref vec2 pInoutVel, float Elasticity, ref int pBounces)
        {
            vec2 Pos = pInoutPos;
            vec2 Vel = pInoutVel;
            bool grounded = false;
            pBounces = 0;

            if (CheckPoint(Pos + Vel))
            {
                int Affected = 0;
                if (CheckPoint(Pos.x + Vel.x, Pos.y))
                {
                    pInoutVel.x *= -Elasticity;
                    pBounces++;
                    Affected++;
                }

                if (CheckPoint(Pos.x, Pos.y + Vel.y))
                {
                    pInoutVel.y *= -Elasticity;
                    pBounces++;
                    Affected++;
                }

                if (Affected == 0)
                {
                    pInoutVel.x *= -Elasticity;
                    pInoutVel.y *= -Elasticity;
                }
                grounded = true;
            }
            else
            {
                pInoutPos = Pos + Vel;
            }

            return grounded;
        }

        public bool CheckPoint(vec2 Pos)
        {
            return CheckPoint(Pos.x, Pos.y);
        }

        public bool CheckPoint(float x, float y)
        {
            return IsTileSolid(CMath.round_to_int(x), CMath.round_to_int(y));
        }

        public int GetCollisionAt(float x, float y)
        {
            return GetTile(CMath.round_to_int(x), CMath.round_to_int(y));
        }
        
        public bool TestBox(vec2 Pos, vec2 Size)
        {
            Size *= 0.5f;
            if (CheckPoint(Pos.x - Size.x, Pos.y - Size.y))
                return true;
            if (CheckPoint(Pos.x + Size.x, Pos.y - Size.y))
                return true;
            if (CheckPoint(Pos.x - Size.x, Pos.y + Size.y))
                return true;
            if (CheckPoint(Pos.x + Size.x, Pos.y + Size.y))
                return true;
            return false;
        }
        
        public bool MoveBox(ref vec2 pInoutPos, ref vec2 pInoutVel, vec2 Size, float Elasticity)
        {
            bool Grounded = false;
            Elasticity = CMath.clamp(Elasticity, 0, 1);
            // do the move
            vec2 Pos = pInoutPos;
            vec2 Vel = pInoutVel;
            
            float Distance = VMath.length(Vel);
            if (Distance > 0.00001f)
            {
                int Max = (int)Distance;
                float Fraction = 1.0f / (Max + 1);

                for (int i = 0; i <= Max; i++)
                {
                    vec2 NewPos = Pos + Vel * Fraction; // TODO: this row is not nice

                    if (TestBox(new vec2(NewPos.x, NewPos.y), Size))
                    {
                        Grounded = true;
                        int Hits = 0;

                        if (TestBox(new vec2(Pos.x, NewPos.y), Size))
                        {
                            NewPos.y = Pos.y;
                            Vel.y *= -Elasticity;
                            Hits++;
                        }

                        if (TestBox(new vec2(NewPos.x, Pos.y), Size))
                        {
                            NewPos.x = Pos.x;
                            Vel.x *= -Elasticity;
                            Hits++;
                        }

                        // neither of the tests got a collision.
                        // this is a real _corner case_!
                        if (Hits == 0)
                        {
                            NewPos.y = Pos.y;
                            NewPos.x = Pos.x;
                            Vel.y *= -Elasticity;
                            Vel.x *= -Elasticity;
                        }
                    }

                    Pos = NewPos;
                }
            }

            pInoutPos = Pos;
            pInoutVel = Vel;
            return Grounded;
        }

        public CTile GetTileAtIndex(int i)
        {
            return m_pTiles[i];
        }
    }
}
