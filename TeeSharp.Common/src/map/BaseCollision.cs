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
        public virtual TileFlags GetTileFlags(float x, float y)
        {
            return GetTileFlags(MathHelper.RoundToInt(x), MathHelper.RoundToInt(y));
        }

        public abstract TileFlags IntersectLine(Vector2 pos0, Vector2 pos1, out Vector2 outCollision,
            out Vector2 outBeforeCollision);

        public abstract bool TestBox(Vector2 pos, Vector2 size);

        public abstract void MovePoint(ref Vector2 inOutPos, ref Vector2 inOutVel,
            float elasticity, out int bounces);
        public abstract void MoveBox(ref Vector2 pos, ref Vector2 vel, Vector2 boxSize,
            float elasticity);

        public virtual bool IsTileSolid(float x, float y)
        {
            return GetTileFlags(
                MathHelper.RoundToInt(x), 
                MathHelper.RoundToInt(y)
            ).HasFlag(TileFlags.SOLID);
        }

        public virtual bool IsTileSolid(Vector2 pos)
        {
            return IsTileSolid(pos.x, pos.y);
        }
    }
}