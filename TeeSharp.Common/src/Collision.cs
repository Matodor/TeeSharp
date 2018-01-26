using TeeSharp.Map.MapItems;

namespace TeeSharp.Common
{
    public class Collision : BaseCollision
    {
        protected override BaseLayers Layers { get; set; }

        public override void Init(BaseLayers layers)
        {
            Layers = layers;

            for (var i = 0; i < Layers.GameLayer.Width * Layers.GameLayer.Height; i++)
            {
                var tile = Layers.GameLayer.Tiles[i];

                if (tile.Index > 175)
                    continue;

                switch ((MapItems) tile.Index)
                {
                    case MapItems.TILE_DEATH:
                        tile.Index = (byte) CollisionFlag.DEATH;
                        break;

                    case MapItems.TILE_SOLID:
                        tile.Index = (byte)CollisionFlag.SOLID;
                        break;

                    case MapItems.TILE_NOHOOK:
                        tile.Index = (byte) (CollisionFlag.SOLID | CollisionFlag.NOHOOK);
                        break;
                }
            }
        }

        public override Tile GetTileAtIndex(int index)
        {
            return Layers.GameLayer.Tiles[index];
        }
    }
}