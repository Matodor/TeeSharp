using System;
using TeeSharp.Common.Enums;
using TeeSharp.Core;
using TeeSharp.Map.MapItems;

namespace TeeSharp.Common
{
    public abstract class BaseMapCollision : BaseInterface
    {
        public virtual int Width { get; protected set; }
        public virtual int Height { get; protected set; }

        protected virtual BaseMapLayers MapLayers { get; set; }
        protected virtual Tile[] GameLayerTiles { get; set; }

        public abstract void Init(BaseMapLayers mapLayers);
        public abstract Tile GetTile(int index);
        public abstract Tile GetTile(int x, int y);
        public abstract CollisionFlags GetTileFlags(int x, int y);
        public abstract CollisionFlags IntersectLine(Vector2 pos0, Vector2 pos1, out Vector2 outCollision,
            out Vector2 outBeforeCollision);
        public abstract bool TestBox(Vector2 pos, Vector2 size);
        public abstract void MovePoint(ref Vector2 inOutPos, ref Vector2 inOutVel,
            float elasticity, out int bounces);
        public abstract void MoveBox(ref Vector2 pos, ref Vector2 vel, Vector2 boxSize,
            float elasticity);

        protected abstract void SetTilesFlags();

        public virtual CollisionFlags GetTileFlags(float x, float y)
        {
            return GetTileFlags(MathHelper.RoundToInt(x), MathHelper.RoundToInt(y));
        }

        public virtual bool IsTileSolid(float x, float y)
        {
            return GetTileFlags(
                MathHelper.RoundToInt(x), 
                MathHelper.RoundToInt(y)
            ).HasFlag(CollisionFlags.Solid);
        }

        public virtual bool IsTileSolid(Vector2 pos)
        {
            return IsTileSolid(pos.x, pos.y);
        }
    }
}