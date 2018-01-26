using System.IO;
using System.Runtime.InteropServices;
using TeeSharp.Core;
using TeeSharp.Map.MapItems;

namespace TeeSharp.Map
{
    /*
        map format:
            [ 4] item 

     
    */
    public enum MapItemTypes
    {
        VERSION = 0,
        INFO,
        IMAGE,
        ENVELOPE,
        GROUP,
        LAYER,
        ENVPOINTS
    }

    public enum LayerType
    {
        INVALID = 0,
        GAME,
        TILES,
        QUADS
    }

    public class MapContainer
    {
        public const int 
            ENTITY_OFFSET = 255 - 16 * 4,
            TILESLAYERFLAG_GAME = 1;

        public uint CRC { get; }
        public int Size { get; }
        public byte[] RawData { get; }
        public string MapName { get; set; }

        public readonly MapInfo MapInfo;

        private readonly DataFile _dataFile;

        private MapContainer(
            DataFile dataFile, 
            MapInfo mapInfo,
            LayerGroup[] mapGroups)
        {
            _dataFile = dataFile;
            CRC = _dataFile.Crc;
            Size = _dataFile.Raw.Length;
            RawData = _dataFile.Raw;

            MapInfo = mapInfo;
        }

        private static MapInfo LoadMapInfo(DataFile dataFile)
        {
            var mapItemInfo = dataFile.FindItem<MapItemInfo>(
                (int)MapItemTypes.INFO, 0);

            var mapInfo = new MapInfo
            {
                Author = string.Empty,
                Version = string.Empty,
                Credits = string.Empty,
                License = string.Empty
            };

            if (mapItemInfo.Version == 1)
            {
                if (mapItemInfo.Author > -1)
                    mapInfo.Author = dataFile.GetData<char>(mapItemInfo.Author).ToString();
                if (mapItemInfo.MapVersion > -1)
                    mapInfo.Version = dataFile.GetData<char>(mapItemInfo.MapVersion).ToString();
                if (mapItemInfo.Credits > -1)
                    mapInfo.Credits = dataFile.GetData<char>(mapItemInfo.Credits).ToString();
                if (mapItemInfo.License > -1)
                    mapInfo.License = dataFile.GetData<char>(mapItemInfo.License).ToString();
            }

            return mapInfo;
        }

        private static LayerGroup[] LoadMapGroups(DataFile dataFile)
        {
            dataFile.GetType((int)MapItemTypes.LAYER,
                out var layersStart, out var layersNum);

            dataFile.GetType((int)MapItemTypes.GROUP,
                out var groupsStart, out var groupsNum);

            var groups = new LayerGroup[groupsNum];
            for (var g = 0; g < groupsNum; g++)
            {
                var itemGroup = dataFile.GetItem<MapItemGroup>(
                    groupsStart + g, 
                    out var _, 
                    out var _
                );

                if (itemGroup.Version < 1 || itemGroup.Version > MapItemGroup.CURRENT_VERSION)
                    continue;

                groups[g] = new LayerGroup
                {
                    ParallaxX = itemGroup.ParallaxX,
                    ParallaxY = itemGroup.ParallaxY,
                    OffsetX = itemGroup.OffsetX,
                    OffsetY = itemGroup.OffsetY,
                    Layers = new Layer[itemGroup.NumLayers]
                };

                if (itemGroup.Version >= 2)
                {
                    groups[g].UseClipping = itemGroup.UseClipping;
                    groups[g].ClipX = itemGroup.ClipX;
                    groups[g].ClipY = itemGroup.ClipY;
                    groups[g].ClipW = itemGroup.ClipW;
                    groups[g].ClipH = itemGroup.ClipH;
                }

                if (itemGroup.Version >= 3)
                    groups[g].Name = itemGroup.IntName.IntsToStr();

                LoadLayers(dataFile, layersStart, itemGroup, groups[g]);
            }

            return groups;
        }

        public static void LoadLayers(DataFile dataFile, int layersStart,
            MapItemGroup itemGroup, LayerGroup group)
        {
            group.Layers = new Layer[itemGroup.NumLayers];
            for (var l = 0; l < group.Layers.Length; l++)
            {
                var layerItem = dataFile.GetItem<MapItemLayer>(
                    layersStart + itemGroup.StartLayer + l,
                    out var _,
                    out var _
                );

                if (layerItem.Type == (int)LayerType.TILES)
                {
                    var itemLayerTilemap = dataFile.GetItem<MapItemLayerTilemap>(
                        layersStart + itemGroup.StartLayer + l,
                        out var _,
                        out var _
                    );

                    LayerTiles layerTiles = null;

                    if ((itemLayerTilemap.Flags & TILESLAYERFLAG_GAME) != 0)
                    {
                        layerTiles = new LayerGame();
                        group.GameGroup = true;
                        group.Name = "Game";
                    }
                    else
                    {
                        layerTiles = new LayerTiles
                        {
                            Color = itemLayerTilemap.Color,
                            ColorEnv = itemLayerTilemap.ColorEnv,
                            ColorEnvOffset = itemLayerTilemap.ColorEnvOffset
                        };
                    }

                    layerTiles.Image = itemLayerTilemap.Image;
                    layerTiles.Width = itemLayerTilemap.Width;
                    layerTiles.Height = itemLayerTilemap.Height;
                    
                    if (itemLayerTilemap.Version >= 3)
                        layerTiles.Name = itemLayerTilemap.IntName.IntsToStr();

                    layerTiles.Tiles = dataFile.GetData<Tile>(itemLayerTilemap.Data);
                    if (layerTiles.GameTiles && 
                        itemLayerTilemap.Version == MakeVersion(1, itemLayerTilemap))
                    {
                        for (var i = 0; i < layerTiles.Width * layerTiles.Height; i++)
                        {
                            if (layerTiles.Tiles[i].Index > 0)
                                layerTiles.Tiles[i].Index += ENTITY_OFFSET;
                        }
                    }

                    dataFile.UnloadData(itemLayerTilemap.Data);
                    group.Layers[l] = layerTiles;
                }
                else if (layerItem.Type == (int)LayerType.QUADS)
                {
                    var quadsItem = dataFile.GetItem<MapItemLayerQuads>(
                        layersStart + itemGroup.StartLayer + l,
                        out var _,
                        out var _
                    );

                    var layerQuads = new LayerQuads
                    {
                        Image = quadsItem.Image,
                        Name = quadsItem.IntName.IntsToStr(),
                        Quads = dataFile.GetData<Quad>(quadsItem.Data)
                    };

                    dataFile.UnloadData(quadsItem.Data);
                    group.Layers[l] = layerQuads;
                }

                group.Layers[l].Flags = layerItem.Flags;
                group.Layers[l].Type = (LayerType) layerItem.Type;
            }
        }

        public static int MakeVersion<T>(int i, T v)
        {
            return (i << 16) + Marshal.SizeOf<T>();
        }

        public static MapContainer Load(FileStream stream, out string error)
        {
            var dataFile = DataFileReader.Read(stream, out error);
            if (dataFile == null)
                return null;

            var mapItemVersion = dataFile.FindItem<MapItemVersion>(
                (int) MapItemTypes.VERSION, 0);

            if (mapItemVersion.Version != 1)
                return null;

            // load map info
            var mapInfo = LoadMapInfo(dataFile);

            // load images
            //

            // load groups
            var groups = LoadMapGroups(dataFile);

            // load envelopes
            //

            return new MapContainer(
                dataFile,
                mapInfo,
                groups
            );
        }
    }
}
