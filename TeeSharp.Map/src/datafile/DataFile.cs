using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    public class DataFile
    {
        public readonly uint Crc;
        public readonly DataFileVersionHeader VersionHeader;
        public readonly DataFileHeader Header;
        public readonly DataFileItemType[] ItemTypes;
        public readonly DataFileItemOffset[] ItemOffsets;

        public DataFile(uint crc, 
            DataFileVersionHeader versionHeader,
            DataFileHeader header, 
            DataFileItemType[] itemTypes,
            DataFileItemOffset[] itemOffsets)
        {
            Crc = crc;
            VersionHeader = versionHeader;
            Header = header;
            ItemTypes = itemTypes;
            ItemOffsets = itemOffsets;
        }
    }
}