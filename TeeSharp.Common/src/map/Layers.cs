using TeeSharp.Map;
using TeeSharp.Map.MapItems;

namespace TeeSharp.Common
{
    public class Layers : BaseLayers
    {
        public override MapItemGroup GameGroup { get; protected set; }
        public override MapItemLayerTilemap GameLayer { get; protected set; }
        public override MapContainer Map { get; protected set; }

        protected override int GroupsStart { get; set; }
        protected override int GroupsNum { get; set; }
        protected override int LayersStart { get; set; }
        protected override int LayersNum { get; set; }

        public override void Init(MapContainer map)
        {
            Map = map;
            Map.GetType(MapItemTypes.GROUP, out var groupsStart, out var groupsNum);
            Map.GetType(MapItemTypes.LAYER, out var layersStart, out var layersNum);

            GroupsStart = groupsStart;
            GroupsNum = groupsNum;
            LayersStart = layersStart;
            LayersNum = layersNum;

            for (var g = 0; g < GroupsNum; g++)
            {
                var group = GetGroup(g);

                for (var l = 0; l < group.NumLayers; l++)
                {
                    var layer = GetLayer(group.StartLayer + l);

                    if (layer.Type == (int) LayerType.TILES)
                    {
                        var tilemap = Map.GetItem<MapItemLayerTilemap>(
                                LayersStart + group.StartLayer + l, 
                                out _, 
                                out _
                        );

                        if ((tilemap.Flags & MapContainer.TILESLAYERFLAG_GAME) == 0)
                            continue;
                        
                        GameLayer = tilemap;
                        GameGroup = group;

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
                    }
                }
            }
        }

        public override MapItemGroup GetGroup(int index)
        {
            return Map.GetItem<MapItemGroup>(GroupsStart + index, out _, out _);
        }

        public override MapItemLayer GetLayer(int index)
        {
            return Map.GetItem<MapItemLayer>(LayersStart + index, out _, out _);
        }
    }
}