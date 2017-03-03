using System.Collections.Generic;
using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    /*
        Class: Game World
            Tracks all entities in the game. Propagates tick and
            snap calls to all entities.
    */

    public class Pair3<T, U, S> : Pair<T, U>
    {
        public S Third { get; set; }

        public Pair3() { }
        public Pair3(T first, U second, S third) : base(first, second)
        {
            Third = third;
        }
    }

    public class Pair<T, U>
    {
        public Pair() { }
        public Pair(T first, U second)
        {
            First = first;
            Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }
    }

    public class CGameWorld
    {
        private readonly CConfiguration g_Config = CConfiguration.Instance;
        public const int
            ENTTYPE_PROJECTILE = 0,
            ENTTYPE_LASER = 1,
            ENTTYPE_PICKUP = 2,
            ENTTYPE_FLAG = 3,
            ENTTYPE_CHARACTER = 4,
            ENTTYPE_DROPITEM = 5,
            ENTTYPE_TRANSPORT = 6,
            ENTTYPE_LASERDOOR = 7,
            ENTTYPE_WORK_MINE_RESOURCE = 8,
            ENTTYPE_LINE_LASER = 9,

            NUM_ENTTYPES = 10;

        private CEntity m_pNextTraverseEntity;
        private readonly CEntity[] m_apFirstEntityTypes = new CEntity[NUM_ENTTYPES];

        private CGameContext m_pGameServer;
        private IServer m_pServer;

        public CGameContext GameServer
        {
            get { return m_pGameServer; }
        }

        public IServer Server
        {
            get { return m_pServer; }
        }

        public readonly CCharacterCore[] m_apCharacters;
        public bool m_ResetRequested;
        public bool m_Paused;

        public CGameWorld()
        {
            m_apCharacters = new CCharacterCore[(int)Consts.MAX_CLIENTS];
            m_pGameServer = null;
            m_pServer = null;

            m_Paused = false;
            m_ResetRequested = false;

            for (int i = 0; i < NUM_ENTTYPES; i++)
                m_apFirstEntityTypes[i] = null;
        }

        ~CGameWorld()
        {
            // delete all entities
            for (int i = 0; i < NUM_ENTTYPES; i++)
                while (m_apFirstEntityTypes[i] != null)
                    m_apFirstEntityTypes[i] = null;
        }

        public void SetGameServer(CGameContext pGameServer)
        {
            m_pGameServer = pGameServer;
            m_pServer = m_pGameServer.Server;
        }

        public CEntity FindFirst(int Type)
        {
            return Type < 0 || Type >= NUM_ENTTYPES ? null : m_apFirstEntityTypes[Type];
        }

        /*
            Function: find_entities
                Finds entities close to a position and returns them in a list.

            Arguments:
                pos - Position.
                radius - How close the entities have to be.
                ents - Pointer to a list that should be filled with the pointers
                    to the entities.
                max - Number of entities that fits into the ents array.
                type - Type of the entities to find.

            Returns:
                Number of entities found and added to the ents array.
        */

        public int FindEntities(vec2 Pos, float Radius, ref CEntity[] ppEnts, int Max, int Type)
        {
            if (Type < 0 || Type >= NUM_ENTTYPES)
                return 0;

            int Num = 0;
            for (CEntity pEnt = m_apFirstEntityTypes[Type]; pEnt != null; pEnt = pEnt.m_pNextTypeEntity)
            {
                if (VMath.distance(pEnt.m_Pos, Pos) < Radius + pEnt.m_ProximityRadius)
                {
                    if (ppEnts != null)
                        ppEnts[Num] = pEnt;
                    Num++;
                    if (Num == Max)
                        break;
                }
            }

            return Num;
        }

        /*
            Function: interserct_CCharacter
                Finds the closest CCharacter that intersects the line.

            Arguments:
                pos0 - Start position
                pos2 - End position
                radius - How for from the line the CCharacter is allowed to be.
                new_pos - Intersection position
                notthis - Entity to ignore intersecting with

            Returns:
                Returns a pointer to the closest hit or NULL of there is no intersection.
        */

        public CCharacter IntersectCharacter(vec2 Pos0, vec2 Pos1, float Radius, ref vec2 NewPos, CEntity pNotThis = null)
        {
            // Find other players
            float ClosestLen = VMath.distance(Pos0, Pos1) * 100.0f;
            CCharacter pClosest = null;
            CCharacter p = (CCharacter)FindFirst(ENTTYPE_CHARACTER);
            for (; p != null; p = (CCharacter)p.TypeNext())
            {
                if (p == pNotThis)
                    continue;

                vec2 IntersectPos = VMath.closest_point_on_line(Pos0, Pos1, p.m_Pos);
                float Len = VMath.distance(p.m_Pos, IntersectPos);
                if (Len < p.m_ProximityRadius + Radius)
                {
                    Len = VMath.distance(Pos0, IntersectPos);
                    if (Len < ClosestLen)
                    {
                        NewPos = IntersectPos;
                        ClosestLen = Len;
                        pClosest = p;
                    }
                }
            }

            return pClosest;
        }

        /*
		    Function: closest_CCharacter
			    Finds the closest CCharacter to a specific point.

		    Arguments:
			    pos - The center position.
			    radius - How far off the CCharacter is allowed to be
			    notthis - Entity to ignore

		    Returns:
			    Returns a pointer to the closest CCharacter or NULL if no CCharacter is close enough.
	    */

        public CCharacter ClosestCharacter(vec2 Pos, float Radius, CEntity pNotThis)
        {
            // Find other players
            float ClosestRange = Radius * 2;
            CCharacter pClosest = null;

            CCharacter p = (CCharacter)FindFirst(ENTTYPE_CHARACTER);
            for (; p != null; p = (CCharacter)p.TypeNext())
            {
                if (p == pNotThis)
                    continue;

                float Len = VMath.distance(Pos, p.m_Pos);
                if (Len < p.m_ProximityRadius + Radius)
                {
                    if (Len < ClosestRange)
                    {
                        ClosestRange = Len;
                        pClosest = p;
                    }
                }
            }

            return pClosest;
        }

        /*
            Function: insert_entity
                Adds an entity to the world.

            Arguments:
                entity - Entity to add
        */

        public void InsertEntity(CEntity pEnt)
        {
            // insert it
            if (m_apFirstEntityTypes[pEnt.m_ObjType] != null)
                m_apFirstEntityTypes[pEnt.m_ObjType].m_pPrevTypeEntity = pEnt;
            pEnt.m_pNextTypeEntity = m_apFirstEntityTypes[pEnt.m_ObjType];
            pEnt.m_pPrevTypeEntity = null;
            m_apFirstEntityTypes[pEnt.m_ObjType] = pEnt;
        }

        /*
            Function: remove_entity
                Removes an entity from the world.

            Arguments:
                entity - Entity to remove
        */

        public void RemoveEntity(CEntity pEnt)
        {
            // not in the list
            if (pEnt.m_pNextTypeEntity == null && pEnt.m_pPrevTypeEntity == null && m_apFirstEntityTypes[pEnt.m_ObjType] != pEnt)
                return;

            // remove
            if (pEnt.m_pPrevTypeEntity != null)
                pEnt.m_pPrevTypeEntity.m_pNextTypeEntity = pEnt.m_pNextTypeEntity;
            else
                m_apFirstEntityTypes[pEnt.m_ObjType] = pEnt.m_pNextTypeEntity;
            if (pEnt.m_pNextTypeEntity != null)
                pEnt.m_pNextTypeEntity.m_pPrevTypeEntity = pEnt.m_pPrevTypeEntity;

            // keep list traversing valid
            if (m_pNextTraverseEntity == pEnt)
                m_pNextTraverseEntity = pEnt.m_pNextTypeEntity;

            pEnt.m_pNextTypeEntity = null;
            pEnt.m_pPrevTypeEntity = null;
        }

        /*
            Function: destroy_entity
                Destroys an entity in the world.

            Arguments:
                entity - Entity to destroy
        */

        public void DestroyEntity(CEntity pEntity)
        {
            pEntity.m_MarkedForDestroy = true;
        }

        /*
            Function: snap
                Calls snap on all the entities in the world to create
                the snapshot.

            Arguments:
                snapping_client - ID of the client which snapshot
                is being created.
        */

        public void Snap(int SnappingClient)
        {
            for (int i = 0; i < NUM_ENTTYPES; i++)
            {
                for (CEntity pEnt = m_apFirstEntityTypes[i]; pEnt != null;)
                {
                    m_pNextTraverseEntity = pEnt.m_pNextTypeEntity;
                    pEnt.Snap(SnappingClient);
                    pEnt = m_pNextTraverseEntity;
                }
            }
        }

        /*
            Function: tick
                Calls tick on all the entities in the world to progress
                the world to the next tick.

        */

        public void Reset()
        {
            // reset all entities
            for (int i = 0; i < NUM_ENTTYPES; i++)
            {
                for (CEntity pEnt = m_apFirstEntityTypes[i]; pEnt != null;)
                {
                    m_pNextTraverseEntity = pEnt.m_pNextTypeEntity;
                    pEnt.Reset();
                    pEnt = m_pNextTraverseEntity;
                }
            }
            RemoveEntities();

            GameServer.Controller.PostReset();
            RemoveEntities();

            m_ResetRequested = false;
        }

        public void RemoveEntities()
        {
            // destroy objects marked for destruction
            for (int i = 0; i < NUM_ENTTYPES; i++)
            {
                for (CEntity pEnt = m_apFirstEntityTypes[i]; pEnt != null;)
                {
                    m_pNextTraverseEntity = pEnt.m_pNextTypeEntity;
                    if (pEnt.m_MarkedForDestroy)
                    {
                        RemoveEntity(pEnt);
                        pEnt.Destroy();
                    }
                    pEnt = m_pNextTraverseEntity;
                }
            }
        }

        public void Tick()
        {
            if (m_ResetRequested)
                Reset();

            if (!m_Paused)
            {
                // update all objects
                for (int i = 0; i < NUM_ENTTYPES; i++)
                {
                    for (CEntity pEnt = m_apFirstEntityTypes[i]; pEnt != null;)
                    {
                        m_pNextTraverseEntity = pEnt.m_pNextTypeEntity;
                        pEnt.Tick();
                        pEnt = m_pNextTraverseEntity;
                    }
                }

                for (int i = 0; i < NUM_ENTTYPES; i++)
                {
                    for (CEntity pEnt = m_apFirstEntityTypes[i]; pEnt != null;)
                    {
                        m_pNextTraverseEntity = pEnt.m_pNextTypeEntity;
                        pEnt.TickDefered();
                        pEnt = m_pNextTraverseEntity;
                    }
                }
            }

            RemoveEntities();
            UpdatePlayerMaps();
        }

        private int distCompare(Pair<float, int> a, Pair<float, int> b)
        {
            if (a.First < b.First)
                return -1;
            if (a.First == b.First)
                return 0;
            if (a.First > b.First)
                return 1;
            return 0;
        }

        private void UpdatePlayerMaps()
        {
            if (Server.Tick() % g_Config.GetInt("SvMapUpdateRate") != 0)
                return;
            
            const int MAX_CLIENTS = (int)Consts.MAX_CLIENTS;
            const int VANILLA_MAX_CLIENTS = (int)Consts.VANILLA_MAX_CLIENTS;

            List<Pair<float, int>> dist = new List<Pair<float, int>>(MAX_CLIENTS);
            for (int i = 0; i < MAX_CLIENTS; i++) dist.Add(new Pair<float, int>());

            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                if (!Server.ClientIngame(i))
                    continue;
                int idMap = Server.GetIdMap(i);

                // compute distances
                for (int j = 0; j < MAX_CLIENTS; j++)
                {
                    dist[j].Second = j;
                    if (!Server.ClientIngame(j) || GameServer.m_apPlayers[j] == null)
                    {
                        dist[j].First = (float)1e10;
                        continue;
                    }

                    CCharacter ch = GameServer.m_apPlayers[j].GetCharacter();
                    if (ch == null)
                    {
                        dist[j].First = (float)1e9;
                        continue;
                    }

                    // copypasted chunk from character.cpp Snap() follows
                    CCharacter SnapChar = GameServer.GetPlayerChar(i);
                    if (SnapChar != null && GameServer.m_apPlayers[i].GetTeam() != -1 && GameServer.m_apPlayers[i].m_ClientVersion == (int)Consts.VERSION_VANILLA)
                        dist[j].First = (float)1e8;
                    else
                        dist[j].First = 0;

                    dist[j].First += VMath.distance(GameServer.m_apPlayers[i].m_ViewPos, GameServer.m_apPlayers[j].GetCharacter().m_Pos);
                }

                // always send the player himself
                dist[i].First = 0;
                //Server.IdMap[idMap + 0] = ClientID;

                // compute reverse map
                int[] rMap = new int[MAX_CLIENTS];
                for (int j = 0; j < MAX_CLIENTS; j++)
                {
                    rMap[j] = -1;
                }
                for (int j = 0; j < VANILLA_MAX_CLIENTS; j++)
                {
                    if (Server.IdMap[idMap + j] == -1)
                        continue;
                    if (dist[Server.IdMap[idMap + j]].First > 1e9)
                        Server.IdMap[idMap + j] = -1;
                    else rMap[Server.IdMap[idMap + j]] = j;
                }

                dist.Sort(distCompare);

                int mapc = 0;
                int demand = 0;
                for (int j = 0; j < VANILLA_MAX_CLIENTS - 1; j++)
                {
                    int k = dist[j].Second;
                    if (rMap[k] != -1 || dist[j].First > 5e9) continue;
                    while (mapc < VANILLA_MAX_CLIENTS && Server.IdMap[idMap + mapc] != -1) mapc++;
                    if (mapc < VANILLA_MAX_CLIENTS - 1)
                        Server.IdMap[idMap + mapc] = k;
                    else
                        //if (dist[j].first < 1300) // dont bother freeing up space for players which are too far to be displayed anyway
                        demand++;
                }
                for (int j = MAX_CLIENTS - 1; j > VANILLA_MAX_CLIENTS - 2; j--)
                {
                    int k = dist[j].Second;
                    if (rMap[k] != -1 && demand-- > 0)
                        Server.IdMap[idMap + rMap[k]] = -1;
                }
                Server.IdMap[idMap + VANILLA_MAX_CLIENTS - 1] = -1; // player with empty name to say chat msgs
            }
        }
    }
}
