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
        public enum Error
        {
            NONE = 0,
            WRONG_SIGNATURE
        }

        public static DataFile Read(FileStream fs, out Error error)
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
                error = Error.WRONG_SIGNATURE;
                return null;
            }

            error = Error.NONE;
            return null;
        }
    }
}
 