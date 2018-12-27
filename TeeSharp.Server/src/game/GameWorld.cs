using System;
using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;
using TeeSharp.Core;
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
            WorldCore = new WorldCore(Server.MaxClients, Tuning);
        }

        public override T FindEntity<T>(Predicate<T> predicate)
        {
            foreach (var entity in Entity<T>.Entities)
            {
                if (predicate(entity))
                    return entity;
            }
            return null; 
        }

        public override IEnumerable<T> GetEntities<T>()
        {
            foreach (var entity in Entity<T>.Entities)
            {
                yield return entity;
            }
        }

        public override IEnumerable<T> FindEntities<T>(Vector2 pos, float radius)
        {
            foreach (var entity in Entity<T>.Entities)
            {
                if (MathHelper.Distance(entity.Position, pos) < radius + entity.ProximityRadius)
                    yield return entity;
            }
        }

        public override IEnumerable<T> FindEntities<T>(Predicate<T> predicate)
        {
            foreach (var entity in Entity<T>.Entities)
            {
                if (predicate(entity))
                    yield return entity;
            }
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
            // TODO
        }

        public override void Tick()
        {
            if (!Paused)
            {
                foreach (var entity in Entity.All)
                {
                    entity.Tick();
                }

                foreach (var entity in Entity.All)
                {
                    entity.LateTick();
                }
            }
            else
            {
                foreach (var entity in Entity.All)
                {
                    entity.TickPaused();
                }
            }
        }

        public override void BeforeSnapshot()
        {
        }

        public override void OnSnapshot(int snappingClient)
        {
            foreach (var entity in Entity.All)
            {
                entity.OnSnapshot(snappingClient);
            }
        }

        public override void AfterSnapshot()
        {
        }
    }
}