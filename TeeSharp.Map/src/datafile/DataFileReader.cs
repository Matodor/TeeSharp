using System.IO;
using TeeSharp.Core;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Map
{
    /* 
     data file format:
        [  8] version_header
        [ 28] header
        [*12] item_types
        [* 4] item_offsets
        [* 4] data_offsets
        [* 4] _data_sizes
        [   ] items
        [   ] data
    */

    public static class DataFileReader
    {
        public static DataFile Read(Stream stream, out string error)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);

            var crc = Crc32.ComputeChecksum(buffer);
            var header = stream.Read<DataFileHeader>();

            if (header.IdString != "DATA" && header.IdString != "ATAD")
            {
                error = $"wrong signature ({header.IdString})";
                return null;
            }

            if (header.Version != 4)
            {
                error = $"wrong version ({header.Version})";
                return null;
            }

            var itemTypes = stream.ReadArray<DataFileItemType[]>(header.NumItemTypes);
            var itemOffsets = stream.ReadArray<int[]>(header.NumItems);
            var dataOffsets = stream.ReadArray<int[]>(header.NumData);
            var dataSizes = stream.ReadArray<int[]>(header.NumData);
            var itemStartIndex = stream.Position;

            error = string.Empty;
            return new DataFile(
                buffer,
                crc, 
                header, 
                itemTypes, 
                itemOffsets,
                dataOffsets,
                dataSizes,
                (int) itemStartIndex
            );
        }
    }
}
 