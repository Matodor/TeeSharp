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
            GameLayerTiles = Layers.Map.GetData<Tile>(Layers.GameLayer.Data);

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
                flags == (TileFlags.SOLID | TileFlags.NOHOOK) ||
                flags == TileFlags.DEATH)
            {
                return flags;
            }

            return TileFlags.NONE;
        }

        public override bool IsTileSolid(float x, float y)
        {
            return GetTileFlags(
                Math.RoundToInt(x),
                Math.RoundToInt(y)).HasFlag(TileFlags.SOLID);
        }

        public override bool IsTileSolid(vec2 pos)
        {
            return IsTileSolid(pos.x, pos.y);
        }

        public override Tile GetTileAtIndex(int index)
        {
            return GameLayerTiles[index];
        }
    }
}