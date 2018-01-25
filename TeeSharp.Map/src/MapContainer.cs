using System.IO;

namespace TeeSharp.Map
{
    public class MapContainer
    {
        public string MapName { get; set; }
        public uint CRC => _dataFile.Crc;
        public int Size => _dataFile.Raw.Length;
        public byte[] Data => _dataFile.Raw;

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
            return new MapContainer(dataFile);
        }
    }
}
