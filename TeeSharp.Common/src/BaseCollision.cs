using System;
using TeeSharp.Core;
using TeeSharp.Map.MapItems;

namespace TeeSharp.Common
{
    [Flags]
    public enum CollisionFlag
    {
        NONE = 0,
        SOLID = 1 << 0,
        DEATH = 1 << 1,
        NOHOOK = 1 << 2
    }

    public abstract class BaseCollision : BaseInterface
    {
        protected abstract BaseLayers Layers { get; set; }

        public abstract void Init(BaseLayers layers);
        public abstract Tile GetTileAtIndex(int index);
    }
}