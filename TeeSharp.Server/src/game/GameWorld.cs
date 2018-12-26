using System;
using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
    public class GameWorld : BaseGameWorld
    {
        public GameWorld()
        {
            GameContext = Kernel.Get<BaseGameContext>();
            Server = Kernel.Get<BaseServer>();
            Config = Kernel.Get<BaseConfig>();
            Tuning = Kernel.Get<BaseTuningParams>();

            Entities = new List<Entity>();
            WorldCore = new WorldCore(Server.MaxClients, Tuning);
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

        public override IEnumerable<T> FindEntities<T>(Vector2 pos, float radius)
        {
            var current = Entity<T>.FirstTypeEntity;
            while (current != null)
            {
                if (MathHelper.Distance(current.Position, pos) < radius + current.ProximityRadius)
                    yield return (T) current;
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

        public override Character IntersectCharacter(Vector2 pos1, Vector2 pos2, 
            float radius, ref Vector2 newPos, Character notThis)
        {
            var closestLength = MathHelper.Distance(pos1, pos2) * 100f;
            Character closest = null;

            foreach (var character in GetEntities<Character>())
            {
                if (character == notThis)
                    continue;

                var intersectPos = MathHelper.ClosestPointOnLine(pos1, pos2, character.Position);
                var length = MathHelper.Distance(character.Position, intersectPos);
                if (length < character.ProximityRadius + radius)
                {
                    length = MathHelper.Distance(pos1, intersectPos);
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

        public override T ClosestEntity<T>(Vector2 pos, float radius, T notThis)
        {
            var closestRange = radius * 2f;
            T closest = null;

            foreach (var entity in GetEntities<T>())
            {
                if (entity == notThis)
                    continue;

                var len = MathHelper.Distance(pos, entity.Position);
                if (len < entity.ProximityRadius + radius)
                {
                    if (len < closestRange)
                    {
                        closestRange = len;
                        closest = entity;
                    }
                }
            }

            return closest;
        }

        public override void Reset()
        {
            for (var i = Entities.Count - 1; i >= 0; i--)
            {
                Entities[i].Reset();
            }

            RemoveEntities();
            GameContext.GameController.OnReset();
            RemoveEntities();

            ResetRequested = false;
        }

        public override void RemoveEntities()
        {
            for (var i = Entities.Count - 1; i >= 0; i--)
            {
                if (Entities[i].MarkedForDestroy)
                    Entities.RemoveAt(i);
            }
        }

        public override void Tick()
        {
            if (ResetRequested)
                Reset();

            if (!Paused)
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
        }

        public override void BeforeSnapshot()
        {
            // TODO   
        }

        public override void OnSnapshot(int snappingClient)
        {
            for (var i = 0; i < Entities.Count; i++)
            {
                Entities[i].OnSnapshot(snappingClient);
            }
        }

        public override void AfterSnapshot()
        {
            // TODO   
        }
    }
}