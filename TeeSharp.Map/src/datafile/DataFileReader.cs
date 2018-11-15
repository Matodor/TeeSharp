using System;
using System.IO;
using TeeSharp.Core;

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
            var versionHeader = stream.ReadStruct<DataFileVersionHeader>();

            if (versionHeader.Magic != "DATA" && versionHeader.Magic != "ATAD")
            {
                error = $"wrong signature ({versionHeader.Magic})";
                return null;
            }

            if (versionHeader.Version != 4)
            {
                error = $"wrong version ({versionHeader.Version})";
                return null;
            }

            var header = stream.ReadStruct<DataFileHeader>();

            var itemTypes = new DataFileItemType[header.NumItemTypes];
            for (var i = 0; i < itemTypes.Length; i++)
                itemTypes[i] = stream.ReadStruct<DataFileItemType>();

            var itemOffsets = new int[header.NumItems];
            for (var i = 0; i < itemOffsets.Length; i++)
                itemOffsets[i] = stream.ReadStruct<int>();

            var dataOffsets = new int[header.NumData];
            for (var i = 0; i < dataOffsets.Length; i++)
                dataOffsets[i] = stream.ReadStruct<int>();

            var dataSizes = new int[header.NumData];
            for (var i = 0; i < dataSizes.Length; i++)
                dataSizes[i] = stream.ReadStruct<int>();

            var itemStartIndex = stream.Position;

            error = string.Empty;
            return new DataFile(
                buffer,
                crc, 
                versionHeader, 
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
 