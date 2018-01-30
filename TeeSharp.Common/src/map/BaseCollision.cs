using System;
using TeeSharp.Core;
using TeeSharp.Map.MapItems;

namespace TeeSharp.Common
{
    [Flags]
    public enum TileFlags
    {
        NONE = 0,
        SOLID = 1 << 0,
        DEATH = 1 << 1,
        NOHOOK = 1 << 2
    }

    public abstract class BaseCollision : BaseInterface
    {
        public abstract int Width { get; protected set; }
        public abstract int Height { get; protected set; }

        protected abstract BaseLayers Layers { get; set; }
        protected abstract Tile[] GameLayerTiles { get; set; }

        public abstract void Init(BaseLayers layers);
        public abstract Tile GetTileAtIndex(int index);
        public abstract TileFlags GetTileFlags(int x, int y);

        public abstract TileFlags IntersectLine(Vec2 pos0, Vec2 pos1, out Vec2 outCollision,
            out Vec2 outBeforeCollision);

        public abstract bool TestBox(Vec2 pos, Vec2 size);
        public abstract void MoveBox(ref Vec2 vec2, ref Vec2 vel1, Vec2 boxSize,
            float elasticity);

        public virtual TileFlags GetTileFlags(float x, float y)
        {
            return GetTileFlags(Math.RoundToInt(x), Math.RoundToInt(y));
        }

        public virtual bool IsTileSolid(float x, float y)
        {
            return GetTileFlags(
                Math.RoundToInt(x), 
                Math.RoundToInt(y)
            ).HasFlag(TileFlags.SOLID);
        }

        public virtual bool IsTileSolid(Vec2 pos)
        {
            return IsTileSolid(pos.x, pos.y);
        }
    }
}