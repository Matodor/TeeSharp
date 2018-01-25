using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    public class DataFile
    {
        public byte[] Raw;
        public readonly uint Crc;
        public readonly DataFileVersionHeader VersionHeader;
        public readonly DataFileHeader Header;
        public readonly DataFileItemType[] ItemTypes;
        public readonly int[] ItemOffsets;
        public readonly int[] DataOffsets;
        public readonly int[] DataSizes;

        public DataFile(
            byte[] raw,
            uint crc, 
            DataFileVersionHeader versionHeader,
            DataFileHeader header, 
            DataFileItemType[] itemTypes,
            int[] itemOffsets,
            int[] dataOffsets,
            int[] dataSizes)
        {
            Raw = raw;
            Crc = crc;
            VersionHeader = versionHeader;
            Header = header;
            ItemTypes = itemTypes;
            ItemOffsets = itemOffsets;
            DataOffsets = dataOffsets;
            DataSizes = dataSizes;
        }
    }
}