using System.Collections.Generic;
using TeeSharp.Common;

namespace TeeSharp.Server.Game
{
    public static class EntityExtensions
    {
        public static IEnumerable<T> Find<T>(this IEnumerable<T> entities, Vector2 position, float radius) where T : Entity<T>
        {
            foreach (var entity in entities)
            {
                if (MathHelper.Distance(entity.Position, position) < radius + entity.ProximityRadius)
                    yield return entity;
            }
        }

        public static T Closest<T>(this IEnumerable<T> entities, Vector2 pos, float radius, T notThis) where T : Entity<T>
        {
            var closestRange = radius * 2f;
            T closest = null;

            foreach (var entity in entities)
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
    }
}