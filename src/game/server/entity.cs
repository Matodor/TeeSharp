using System;
using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    public abstract class CEntity
    {
        public bool m_MarkedForDestroy;
        public int m_ObjType;

        public CGameWorld GameWorld
        {
            get { return m_pGameWorld; }
        }

        public CGameContext GameServer
        {
            get { return m_pGameWorld.GameServer; }
        }

        public IServer Server
        {
            get { return m_pGameWorld.Server; }
        }

        /*
            Variable: proximity_radius
                Contains the physical size of the entity.
        */
        public float m_ProximityRadius;
        /*
		    Variable: pos
			    Contains the current posititon of the entity.
	    */
        public vec2 m_Pos;

        public CEntity TypeNext() { return m_pNextTypeEntity; }
        public CEntity TypePrev() { return m_pPrevTypeEntity; }

        public CEntity m_pPrevTypeEntity;
        public CEntity m_pNextTypeEntity;

        protected int[] m_IDs;
        private readonly CGameWorld m_pGameWorld;
        private bool m_Destroyed;

        protected CEntity(CGameWorld pGameWorld, int ObjType)
        {
            m_pGameWorld = pGameWorld;

            m_ObjType = ObjType;
            m_Pos = new vec2(0, 0);
            m_ProximityRadius = 0;

            m_MarkedForDestroy = false;
            m_IDs = new int[1];
            for (int i = 0; i < m_IDs.Length; i++)
                m_IDs[i] = Server.SnapNewID();

            m_Destroyed = false;
            m_pPrevTypeEntity = null;
            m_pNextTypeEntity = null;
        }

        protected void SnapNewIDs(int count)
        {
            Array.Resize(ref m_IDs, count);
            for (int i = 1; i < m_IDs.Length; i++)
                m_IDs[i] = Server.SnapNewID();
        }

        ~CEntity()
        {
            if (!m_Destroyed)
            {
                GameWorld.RemoveEntity(this);
                for (int i = 0; i < m_IDs.Length; i++)
                    Server.SnapFreeID(m_IDs[i]);
                m_Destroyed = true;
            }
        }

        /*
            Function: networkclipped(int snapping_client)
                Performs a series of test to see if a client can see the
                entity.

            Arguments:
                snapping_client - ID of the client which snapshot is
                    being generated. Could be -1 to create a complete
                    snapshot of everything in the game for demo
                    recording.

            Returns:
                Non-zero if the entity doesn't have to be in the snapshot.
        */
        public bool NetworkClipped(int SnappingClient)
        {
            return NetworkClipped(SnappingClient, m_Pos);
        }

        public bool NetworkClipped(int SnappingClient, vec2 CheckPos)
        {
            if (SnappingClient == -1)
                return false;

            float dx = GameServer.m_apPlayers[SnappingClient].m_ViewPos.x - CheckPos.x;
            float dy = GameServer.m_apPlayers[SnappingClient].m_ViewPos.y - CheckPos.y;

            if (Math.Abs(dx) > 900.0f || Math.Abs(dy) > 700.0f)
                return true;

            if (VMath.distance(GameServer.m_apPlayers[SnappingClient].m_ViewPos, CheckPos) > 1100.0f)
                return true;
            return false;
        }

        public bool GameLayerClipped(vec2 CheckPos)
        {
            return CMath.round_to_int(CheckPos.x) / 32 < -200 || CMath.round_to_int(CheckPos.x) / 32 > GameServer.Collision.GetWidth() + 200 ||
                   CMath.round_to_int(CheckPos.y) / 32 < -200 || CMath.round_to_int(CheckPos.y) / 32 > GameServer.Collision.GetHeight() + 200;
        }

        /*
            Function: destroy
                Destorys the entity.
        */

        public virtual void Destroy()
        {
            if (!m_Destroyed)
            {
                GameWorld.RemoveEntity(this);
                for (int i = 0; i < m_IDs.Length; i++)
                    Server.SnapFreeID(m_IDs[i]);
                m_Destroyed = true;
            }
        }

        /*
            Function: reset
                Called when the game resets the map. Puts the entity
                back to it's starting state or perhaps destroys it.
        */
        public virtual void Reset() { }

        /*
            Function: tick
                Called progress the entity to the next tick. Updates
                and moves the entity to it's new state and position.
        */
        public virtual void Tick() { }

        /*
            Function: tick_defered
                Called after all entities tick() function has been called.
        */
        public virtual void TickDefered() { }

        /*
            Function: TickPaused
                Called when the game is paused, to freeze the state and position of the entity.
        */
        public virtual void TickPaused() { }

        /*
            Function: snap
                Called when a new snapshot is being generated for a specific
                client.

            Arguments:
                snapping_client - ID of the client which snapshot is
                    being generated. Could be -1 to create a complete
                    snapshot of everything in the game for demo
                    recording.
        */
        public virtual void Snap(int SnappingClient) { }
    }
}
