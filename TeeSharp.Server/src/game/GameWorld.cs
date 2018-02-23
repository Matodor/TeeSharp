using System;
using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;
using TeeSharp.Server.Game.Entities;
using Math = TeeSharp.Common.Math;

namespace TeeSharp.Server.Game
{
    public class GameWorld : BaseGameWorld
    {
        protected virtual List<Pair<float, int>> PlayersDistances { get; set; }

        public GameWorld()
        {
            Entities = new List<Entity>();
            WorldCore = new WorldCore(Server.MaxClients, Tuning);
            PlayersDistances = new List<Pair<float, int>>(Server.MaxClients);

            for (var i = 0; i < Server.MaxClients; i++)
                PlayersDistances.Add(new Pair<float, int>(0, 0));
        }

        public override T FindEntity<T>(Predicate<Entity<T>> predicate)
        {
            var current = Entity<T>.FirstTypeEntity;
            while (current != null)
            {
                if (predicate(current))
                    return (T)current;
                current = current.NextTypeEntity;
            }

            return null; 
        }

        public override IEnumerable<T> GetEntities<T>()
        {
            var current = Entity<T>.FirstTypeEntity;
            while (current != null)
            {
                yield return (T) current;
                current = current.NextTypeEntity;
            }
        }

        public override IEnumerable<T> FindEntities<T>(Vec2 pos, float radius)
        {
            var current = Entity<T>.FirstTypeEntity;
            while (current != null)
            {
                if (Math.Distance(current.Position, pos) < radius + current.ProximityRadius)
                    yield return (T)current;
                current = current.NextTypeEntity;
            }
        }

        public override IEnumerable<T> FindEntities<T>(Predicate<Entity<T>> predicate)
        {
            var current = Entity<T>.FirstTypeEntity;
            while (current != null)
            {
                if (predicate(current))
                    yield return (T)current;
                current = current.NextTypeEntity;
            }
        }

        public override void AddEntity<T>(Entity<T> entity)
        {
            if (Entity<T>.FirstTypeEntity != null)
                Entity<T>.FirstTypeEntity.PrevTypeEntity = entity;

            entity.NextTypeEntity = Entity<T>.FirstTypeEntity;
            entity.PrevTypeEntity = null;

            Entity<T>.FirstTypeEntity = entity;
            Entities.Add(entity);
        }

        public override void RemoveEntity<T>(Entity<T> entity)
        {
            if (entity.PrevTypeEntity == null)
                Entity<T>.FirstTypeEntity = entity.NextTypeEntity;
            else
                entity.PrevTypeEntity.NextTypeEntity = entity.NextTypeEntity;

            if (entity.NextTypeEntity != null)
                entity.NextTypeEntity.PrevTypeEntity = entity.PrevTypeEntity;

            entity.NextTypeEntity = null;
            entity.PrevTypeEntity = null;
        }

        public override Character IntersectCharacter(Vec2 pos1, Vec2 pos2, 
            float radius, ref Vec2 newPos, Character notThis)
        {
            var closestLength = Math.Distance(pos1, pos2) * 100f;
            Character closest = null;

            foreach (var character in GetEntities<Character>())
            {
                if (character == notThis)
                    continue;

                var intersectPos = Math.ClosestPointOnLine(pos1, pos2, character.Position);
                var length = Math.Distance(character.Position, intersectPos);
                if (length < character.ProximityRadius + radius)
                {
                    length = Math.Distance(pos1, intersectPos);
                    if (length < closestLength)
                    {
                        newPos = intersectPos;
                        closestLength = length;
                        closest = character;
                    }
                }
            }

            return closest;
        }

        public override Character ClosestCharacter(Vec2 pos, float radius, Character notThis)
        {
            var closestRange = radius * 2f;
            Character closest = null;

            foreach (var character in GetEntities<Character>())
            {
                if (character == notThis)
                    continue;

                var len = Math.Distance(pos, character.Position);
                if (len < character.ProximityRadius + radius)
                {
                    if (len < closestRange)
                    {
                        closestRange = len;
                        closest = character;
                    }
                }
            }

            return closest;
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }

        public override void RemoteEntities()
        {
            throw new NotImplementedException();
        }

        public override void Tick()
        {
            if (!IsPaused)
            {
                for (var i = Entities.Count - 1; i >= 0; i--)
                {
                    Entities[i].Tick();
                }

                for (var i = Entities.Count - 1; i >= 0; i--)
                {
                    Entities[i].TickDefered();

                    if (Entities[i].MarkedForDestroy)
                        Entities.RemoveAt(i);
                }
            }
            else
            {
                for (var i = Entities.Count - 1; i >= 0; i--)
                {
                    Entities[i].TickPaused();

                    if (Entities[i].MarkedForDestroy)
                        Entities.RemoveAt(i);
                }
            }

            UpdatePlayerMaps();
        }

        private static int DistancesComparer(Pair<float, int> a, Pair<float, int> b)
        {
            if (a.First < b.First)
                return -1;
            if (a.First > b.First)
                return 1;
            return 0;
        }

        protected virtual void UpdatePlayerMaps()
        {
            if (Server.Tick % Config["SvMapUpdateRate"] != 0)
                return;
            
            for (var i = 0; i < PlayersDistances.Count; i++)
            {
                PlayersDistances[i].First = 0;
                PlayersDistances[i].Second = 0;
            }

            for (var i = 0; i < PlayersDistances.Count; i++)
            {
                if (!Server.ClientInGame(i))
                    continue;

                var idMap = BaseServer.GetIdMap(i);
                // compute distances
                for (var j = 0; j < PlayersDistances.Count; j++)
                {
                    PlayersDistances[j].Second = j;
                    if (!Server.ClientInGame(j) || GameContext.Players[j] == null)
                    {
                        PlayersDistances[j].First = (float)1e10;
                        continue;
                    }

                    var character = GameContext.Players[j].GetCharacter();
                    if (character == null)
                    {
                        PlayersDistances[j].First = (float)1e9;
                        continue;
                    }

                    // copypasted chunk from character.cpp Snap() follows
                    if (GameContext.Players[i].GetCharacter() != null &&
                        GameContext.Players[i].Team != Team.SPECTATORS &&
                        GameContext.Players[i].ClientVersion == ClientVersion.VANILLA)
                    {
                        PlayersDistances[j].First = (float) 1e8;
                    }
                    else
                        PlayersDistances[j].First = 0;

                    PlayersDistances[j].First += Math.Distance(
                        GameContext.Players[i].ViewPos,
                        GameContext.Players[j].GetCharacter().Position
                    );
                }

                // always send the player himself
                PlayersDistances[i].First = 0;

                // compute reverse map
                var rMap = new int[PlayersDistances.Count];
                for (var j = 0; j < PlayersDistances.Count; j++)
                    rMap[j] = -1;

                for (var j = 0; j < BaseServer.VANILLA_MAX_CLIENTS; j++)
                {
                    if (Server.IdMap[idMap + j] == -1)
                        continue;
                    if (PlayersDistances[Server.IdMap[idMap + j]].First > 1e9)
                        Server.IdMap[idMap + j] = -1;
                    else
                        rMap[Server.IdMap[idMap + j]] = j;
                }

                PlayersDistances.Sort(DistancesComparer);

                var mapc = 0;
                var demand = 0;

                for (var j = 0; j < BaseServer.VANILLA_MAX_CLIENTS - 1; j++)
                {
                    var k = PlayersDistances[j].Second;
                    if (rMap[k] != -1 || PlayersDistances[j].First > 5e9)
                        continue;

                    while (mapc < BaseServer.VANILLA_MAX_CLIENTS &&
                           Server.IdMap[idMap + mapc] != -1)
                    {
                        mapc++;
                    }

                    if (mapc < BaseServer.VANILLA_MAX_CLIENTS - 1)
                        Server.IdMap[idMap + mapc] = k;
                    else
                        demand++;
                }
                for (var j = PlayersDistances.Count - 1; j > BaseServer.VANILLA_MAX_CLIENTS - 2; j--)
                {
                    var k = PlayersDistances[j].Second;
                    if (rMap[k] != -1 && demand-- > 0)
                        Server.IdMap[idMap + rMap[k]] = -1;
                }
                Server.IdMap[idMap + BaseServer.VANILLA_MAX_CLIENTS - 1] = -1; // player with empty name to say chat msgs
            }
        }

        public override void OnSnapshot(int snappingClient)
        {
            for (var i = 0; i < Entities.Count; i++)
            {
                Entities[i].OnSnapshot(snappingClient);
            }
        }
    }
}