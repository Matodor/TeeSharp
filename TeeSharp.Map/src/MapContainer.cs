using System.IO;
using System.Runtime.InteropServices;
using TeeSharp.Map.MapItems;

namespace TeeSharp.Map
{
    public class MapContainer
    {
        public int Size { get; }
        public uint CRC => _dataFile.Crc;
        public byte[] RawData => _dataFile.Raw;
        public string MapName { get; set; }

        private readonly DataFile _dataFile;

        private MapContainer(DataFile dataFile)
        {
            _dataFile = dataFile;
            Size = _dataFile.Raw.Length;
        }

        public void GetType(MapItemTypes type, out int startItems, out int numItems)
        {
            _dataFile.GetType(type, out startItems, out numItems);
        }

        public T GetItem<T>(int index, out MapItemTypes itemType, out int itemId)
        {
            var item = _dataFile.GetItem<T>(index, out var typeId, out itemId);
            itemType = (MapItemTypes) typeId;
            return item;
        }

        public T GetData<T>(int index)
        {
            return _dataFile.GetData<T>(index);
        }

        public void UnloadData(int index)
        {
            _dataFile.UnloadData(index);
        }

        public static int MakeVersion<T>(int i, T v)
        {
            return (i << 16) + Marshal.SizeOf<T>();
        }

        public static MapContainer Load(Stream stream, out string error)
        {
            var dataFile = DataFileReader.Read(stream, out error);
            var item = dataFile?.FindItem<MapItemVersion>(MapItemTypes.Version, 0);

            if (item == null || item.Version != 1)
                return null;

            dataFile.GetType(MapItemTypes.Group, out var groupsStart, out var groupsNum);
            dataFile.GetType(MapItemTypes.Layer, out var layersStart, out var layersNum);

            for (var g = 0; g < groupsNum; g++)
            {
                var group = dataFile.GetItem<MapItemGroup>(groupsStart + g, out _, out _);
                for (var l = 0; l < group.NumLayers; l++)
                {
                    var layer = dataFile.GetItem<MapItemLayer>(layersStart + group.StartLayer + l, out _, out _);
                    if (layer.Type == LayerType.Tiles)
                    {
                        var tilemap = dataFile.GetItem<MapItemLayerTilemap>(
                            layersStart + group.StartLayer + l,
                            out _,
                            out _
                        );

                        if (tilemap.Version > 3)
                        {
                            var tiles = new Tile[tilemap.Width * tilemap.Height];
                            var savedTiles = dataFile.GetData<Tile[]>(tilemap.Data);
                            var i = 0;
                            var sIndex = 0;

                            while (i < tilemap.Width * tilemap.Height)
                            {
                                for (var counter = 0;
                                    counter <= savedTiles[sIndex].Skip && i < tilemap.Width * tilemap.Height;
                                    counter++)
                                {
                                    tiles[i] = savedTiles[sIndex];
                                    tiles[i++].Skip = 0;
                                }

                                sIndex++;
                            }

                            dataFile.ReplaceData<Tile[]>(tilemap.Data, tiles);
                        }
                    }
                }
            }

            return new MapContainer(dataFile);
        }
    }
}
