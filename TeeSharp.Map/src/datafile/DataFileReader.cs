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
        public static DataFile Read(FileStream fs, out string error)
        {
            uint crc;
            {
                var buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Seek(0, SeekOrigin.Begin);
                crc = Crc32.ComputeChecksum(buffer);
            }

            var versionHeader = fs.ReadStruct<DataFileVersionHeader>();
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

            var header = fs.ReadStruct<DataFileHeader>();

            var itemTypes = new DataFileItemType[header.NumItemTypes];
            for (var i = 0; i < itemTypes.Length; i++)
                itemTypes[i] = fs.ReadStruct<DataFileItemType>();

            var itemOffsets = new DataFileItemOffset[header.NumItems];
            for (var i = 0; i < itemOffsets.Length; i++)
                itemOffsets[i] = fs.ReadStruct<DataFileItemOffset>();

            error = string.Empty;
            return new DataFile(crc, versionHeader, header, itemTypes, itemOffsets);
        }
    }
}
 