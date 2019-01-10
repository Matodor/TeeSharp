﻿using TeeSharp.Common;
using TeeSharp.Common.Config;
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
            WorldCore = new WorldCore(Server.MaxClients, Tuning);
        }

        public override Character IntersectCharacter(Vector2 pos1, Vector2 pos2, 
            float radius, ref Vector2 newPos, Character notThis)
        {
            var closestLength = MathHelper.Distance(pos1, pos2) * 100f;
            var closest = default(Character);

            foreach (var character in Character.Entities)
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

        protected override void Reset()
        {
            foreach (var entity in Entity.All)
            {
                entity.Reset();
            }

            ResetRequested = false;
            OnReset();
        }

        public override void Tick()
        {
            if (ResetRequested)
                Reset();

            if (Paused)
            {
                foreach (var entity in Entity.All)
                {
                    entity.TickPaused();
                }
            }
            else
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