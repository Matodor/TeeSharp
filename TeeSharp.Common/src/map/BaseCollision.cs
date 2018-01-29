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

        public abstract TileFlags GetTileFlags(int x, int y);
        public abstract bool IsTileSolid(float x, float y);
        public abstract bool IsTileSolid(vec2 pos);
        public abstract Tile GetTileAtIndex(int index);
    }
}