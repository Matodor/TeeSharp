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

                switch ((MapItems)GameLayerTiles[i].Index)
                {
                    case MapItems.TILE_DEATH:
                        GameLayerTiles[i].Index = (byte) CollisionFlags.DEATH;
                        break;

                    case MapItems.TILE_SOLID:
                        GameLayerTiles[i].Index = (byte)CollisionFlags.SOLID;
                        break;

                    case MapItems.TILE_NOHOOK:
                        GameLayerTiles[i].Index = (byte) (CollisionFlags.SOLID | CollisionFlags.NOHOOK);
                        break;
                }
            }
        }

        public override Tile GetTileAtIndex(int index)
        {
            return GameLayerTiles[index];
        }
    }
}