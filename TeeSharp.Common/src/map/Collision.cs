using TeeSharp.Map.MapItems;

namespace TeeSharp.Common
{
    public class Collision : BaseCollision
    {
        public override int Width { get; protected set; }
        public override int Height { get; protected set; }

        protected override BaseLayers Layers { get; set; }
        protected override Tile[] GameLayerTiles { get; set; }

        public override void Init(BaseLayers layers)
        {
            Layers = layers;
            Width = Layers.GameLayer.Width;
            Height = Layers.GameLayer.Height;
            GameLayerTiles = Layers.Map.GetData<Tile[]>(Layers.GameLayer.Data);

            for (var i = 0; i < Width * Height; i++)
            {
                if (GameLayerTiles[i].Index > 175)
                    continue;

                switch ((MapItems) GameLayerTiles[i].Index)
                {
                    case MapItems.TILE_DEATH:
                        GameLayerTiles[i].Index = (byte) TileFlags.DEATH;
                        break;

                    case MapItems.TILE_SOLID:
                        GameLayerTiles[i].Index = (byte) TileFlags.SOLID;
                        break;

                    case MapItems.TILE_NOHOOK:
                        GameLayerTiles[i].Index = (byte) (TileFlags.SOLID | TileFlags.NOHOOK);
                        break;
                }
            }
        }

        public override TileFlags GetTileFlags(int x, int y)
        {
            var nx = System.Math.Clamp(x / 32, 0, Width - 1);
            var ny = System.Math.Clamp(y / 32, 0, Height - 1);

            var flags = (TileFlags) GameLayerTiles[ny * Width + nx].Index;
            if (flags == TileFlags.SOLID ||
                flags == (TileFlags.NOHOOK | TileFlags.NONE | TileFlags.SOLID) ||
                flags == TileFlags.DEATH)
            {
                return flags;
            }

            return TileFlags.NONE;
        }

        public override TileFlags IntersectLine(Vector2 pos0, Vector2 pos1, out Vector2 outCollision, out Vector2 outBeforeCollision)
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
            return TileFlags.NONE;
        }

        public override Tile GetTileAtIndex(int index)
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