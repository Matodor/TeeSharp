using TeeSharp.Map;

namespace TeeSharp.Common
{
    public class Layers : BaseLayers
    {
        public override LayerGroup GameGroup { get; protected set; }
        public override LayerGame GameLayer { get; protected set; }

        public override void Init(MapContainer map)
        {
            for (var g = 0; g < map.MapGroups.Length; g++)
            {
                var group = map.MapGroups[g];
                for (var i = 0; i < group.Layers.Length; i++)
                {
                    var layer = group.Layers[i];

                    if (layer.Type != LayerType.TILES ||
                        !(layer is LayerGame gameTiles))
                    {
                        continue;
                    }

                    GameGroup = group;
                    GameLayer = gameTiles;

                    // make sure the game group has standard settings
                    GameGroup.OffsetX = 0;
                    GameGroup.OffsetY = 0;
                    GameGroup.ParallaxX = 100;
                    GameGroup.ParallaxY = 100;

                    if (GameGroup.Version >= 2)
                    {
                        GameGroup.UseClipping = 0;
                        GameGroup.ClipX = 0;
                        GameGroup.ClipY = 0;
                        GameGroup.ClipW = 0;
                        GameGroup.ClipH = 0;
                    }

                    return;
                }
            }
        }
    }
}