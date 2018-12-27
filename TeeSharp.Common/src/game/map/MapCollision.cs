using TeeSharp.Common.Enums;
using TeeSharp.Map.MapItems;

namespace TeeSharp.Common
{
    public class MapCollision : BaseMapCollision
    {
        public MapCollision()
        {
            Width = 0;
            Height = 0;
        }

        public override void Init(BaseMapLayers mapLayers)
        {
            MapLayers = mapLayers;
            Width = MapLayers.GameLayer.Width;
            Height = MapLayers.GameLayer.Height;
            GameLayerTiles = MapLayers.Map.GetData<Tile[]>(MapLayers.GameLayer.Data);

            SetTilesFlags();
        }

        protected override void SetTilesFlags()
        {
            for (var i = 0; i < Width * Height; i++)
            {
                if (GameLayerTiles[i].Index > 128)
                    continue;

                switch ((MapTiles)GameLayerTiles[i].Index)
                {
                    case MapTiles.Death:
                        GameLayerTiles[i].Index = (byte)CollisionFlags.Death;
                        break;

                    case MapTiles.Solid:
                        GameLayerTiles[i].Index = (byte)CollisionFlags.Solid;
                        break;

                    case MapTiles.NoHook:
                        GameLayerTiles[i].Index = (byte)(CollisionFlags.Solid | CollisionFlags.NoHook);
                        break;
                }
            }
        }

        public override CollisionFlags GetTileFlags(int x, int y)
        {
            var nx = System.Math.Clamp(x / 32, 0, Width - 1);
            var ny = System.Math.Clamp(y / 32, 0, Height - 1);

            var flags = (CollisionFlags) GameLayerTiles[ny * Width + nx].Index;
            if (flags == CollisionFlags.Solid ||
                flags == (CollisionFlags.Solid | CollisionFlags.NoHook) ||
                flags == CollisionFlags.Death)
            {
                return flags;
            }

            return CollisionFlags.None;
        }

        public override CollisionFlags IntersectLine(Vector2 pos0, Vector2 pos1, out Vector2 outCollision, out Vector2 outBeforeCollision)
        {
            var distance = MathHelper.Distance(pos0, pos1);
            var end = (int) (distance + 1);
            var last = pos0;

            for (var i = 0; i < end; i++)
            {
                var amount = i / distance;
                var pos = MathHelper.Mix(pos0, pos1, amount);

                if (IsTileSolid(pos.x, pos.y))
                {
                    outCollision = pos;
                    outBeforeCollision = last;
                    return GetTileFlags(pos.x, pos.y);
                }

                last = pos;
            }

            outCollision = pos1;
            outBeforeCollision = pos1;
            return CollisionFlags.None;
        }

        public override Tile GetTile(int x, int y)
        {
            var nx = System.Math.Clamp(x / 32, 0, Width - 1);
            var ny = System.Math.Clamp(y / 32, 0, Height - 1);

            return GetTile(ny * Width + nx);
        }

        public override Tile GetTile(int index)
        {
            return GameLayerTiles[index];
        }

        public override bool TestBox(Vector2 pos, Vector2 size)
        {
            size *= 0.5f;
            if (IsTileSolid(pos.x - size.x, pos.y - size.y))
                return true;
            if (IsTileSolid(pos.x + size.x, pos.y - size.y))
                return true;
            if (IsTileSolid(pos.x - size.x, pos.y + size.y))
                return true;
            if (IsTileSolid(pos.x + size.x, pos.y + size.y))
                return true;
            return false;
        }

        public override void MovePoint(ref Vector2 inOutPos, ref Vector2 inOutVel, 
            float elasticity, out int bounces)
        {
            bounces = 0;

            var pos = inOutPos;
            var vel = inOutVel;

            if (IsTileSolid(pos + vel))
            {
                var affected = 0;

                if (IsTileSolid(pos.x + vel.x, pos.y))
                {
                    inOutVel.x *= -elasticity;
                    bounces++;
                    affected++;
                }

                if (IsTileSolid(pos.x, pos.y + vel.y))
                {
                    inOutVel.y *= -elasticity;
                    bounces++;
                    affected++;
                }

                if (affected == 0)
                {
                    inOutVel.x *= -elasticity;
                    inOutVel.y *= -elasticity;
                }
            }
            else
            {
                inOutPos = pos + vel;
            }
        }

        public override void MoveBox(ref Vector2 pos, ref Vector2 vel, Vector2 boxSize, 
            float elasticity)
        {
            elasticity = System.Math.Clamp(elasticity, 0f, 1f);
            var distance = vel.Length;
            if (distance <= 0.00001f)
                return;

            var max = (int)distance;
            var fraction = 1.0f / (max + 1);

            for (var i = 0; i <= max; i++)
            {
                var newPos = pos + vel * fraction;
                if (TestBox(newPos, boxSize))
                {
                    var hits = 0;
                    if (TestBox(new Vector2(pos.x, newPos.y), boxSize))
                    {
                        newPos.y = pos.y;
                        vel.y *= -elasticity;
                        hits++;
                    }

                    if (TestBox(new Vector2(newPos.x, pos.y), boxSize))
                    {
                        newPos.x = pos.x;
                        vel.x *= -elasticity;
                        hits++;
                    }

                    if (hits == 0)
                    {
                        newPos = pos;
                        vel *= -elasticity;
                    }
                }

                pos = newPos;
            }
        }
    }
}