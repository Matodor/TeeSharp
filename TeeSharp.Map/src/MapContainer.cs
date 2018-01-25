using System.IO;
using TeeSharp.Map.map_items;

namespace TeeSharp.Map
{
    /*
        map format:
            [ 4] item 

     
    */
    public class MapContainer
    {
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

        public string MapName { get; set; }
        public uint CRC => _dataFile.Crc;
        public int Size => _dataFile.Raw.Length;
        public byte[] RawData => _dataFile.Raw;

        private readonly DataFile _dataFile;

        private MapContainer(DataFile dataFile)
        {
            _dataFile = dataFile;
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

            var mapItemInfo = dataFile.FindItem<MapItemInfo>(
                (int) MapItemTypes.INFO, 0);

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
                    mapInfo.Author = dataFile.GetData<string>(mapItemInfo.Author);
                if (mapItemInfo.MapVersion > -1)
                    mapInfo.Version = dataFile.GetData<string>(mapItemInfo.MapVersion);
                if (mapItemInfo.Credits > -1)
                    mapInfo.Credits = dataFile.GetData<string>(mapItemInfo.Credits);
                if (mapItemInfo.License > -1)
                    mapInfo.License = dataFile.GetData<string>(mapItemInfo.License);
            }

            return new MapContainer(dataFile);
        }
    }
}
