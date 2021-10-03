using System.IO;
using TeeSharp.Core.Extensions;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Map
{
    public static class DataFileReader
    {
        public static bool Read(Stream stream, out string error, out DataFile dataFile)
        {
            // TODO READ CRC32
            // TODO READ SHA256
            
            stream.Seek(0, SeekOrigin.Begin);
            dataFile = null;
            
            if (stream.Get<DataFileHeader>(out var header))
            {
                if (!header.IsValidSignature)
                {
                    error = "Wrong map header signature";
                    return false;
                }
            }
            else
            {
                error = "Parse map header error";
                return false;
            }
            
            if (!header.IsValidVersion)
            {
                error = $"Wrong map version ({header.Version})";
                return false;
            }

            // ReSharper disable ArrangeRedundantParentheses
            var fileSize =
                (StructHelper<DataFileHeader>.Size) +
                (header.ItemTypesCount * StructHelper<DataFileItemTypeInfo>.Size) +
                (header.ItemsCount + header.RawDataBlocks + header.RawDataBlocks) * sizeof(int) +
                (header.ItemsSize) +
                (header.RawDataBlocksSize);
            // ReSharper restore ArrangeRedundantParentheses

            if (fileSize != stream.Length)
            {
                error = "Invalid file size";
                return false;
            }
            
            if (!stream.Get<DataFileItemTypeInfo>(header.ItemTypesCount, out var itemTypes))
            {
                error = "Get map item types error";
                return false;
            }

            if (!stream.Get<int>(header.ItemsCount, out var itemsOffsets))
            {
                error = "Get map items offsets error";
                return false;
            }

            if (!stream.Get<int>(header.RawDataBlocks, out var dataOffsets))
            {
                error = "Get map data offsets error";
                return false;
            }
            
            if (!stream.Get<int>(header.RawDataBlocks, out var dataSizes))
            {
                error = "Get map data offsets error";
                return false;
            }

            // ReSharper disable ArgumentsStyleOther
            // ReSharper disable ArgumentsStyleNamedExpression
            // ReSharper disable ArgumentsStyleLiteral
            dataFile = new DataFile(
                stream: stream,
                header: header,
                itemTypes: itemTypes.ToArray(),
                itemsOffsets: itemsOffsets.ToArray(),
                dataOffsets: dataOffsets.ToArray(),
                dataSizes: dataSizes.ToArray(),
                itemsStartOffset: stream.Position,
                dataStartOffset: stream.Position + header.ItemsSize
            );
            // ReSharper restore ArgumentsStyleOther
            // ReSharper restore ArgumentsStyleNamedExpression
            // ReSharper restore ArgumentsStyleLiteral
            
            error = string.Empty;
            return true;
        }
    }
}