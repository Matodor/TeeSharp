using System.IO;
using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    public class MapContainer
    {
        public const int
            TILESLAYERFLAG_GAME = 1;

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
            _dataFile.GetType((int) type, out startItems, out numItems);
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
            if (dataFile == null)
                return null;
            return new MapContainer(dataFile);
        }
    }
}
