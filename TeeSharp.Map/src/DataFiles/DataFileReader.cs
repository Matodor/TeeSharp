using System;
using System.IO;
using TeeSharp.Core;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Map
{
    public static class DataFileReader
    {
        public static bool Read(Stream stream, out string error)
        {
            stream.Seek(0, SeekOrigin.Begin);

            if (stream.GetStruct<DataFileHeader>(out var header))
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
            var dataSize =
                (header.ItemTypesCount * TypeHelper<DataFileItemType>.Size) +
                (header.ItemsCount + header.RawDataSize) * sizeof(int) +
                (header.ItemSize) +
                (header.Version == 4 ? header.RawDataSize * sizeof(int) : 0);
            // ReSharper restore ArrangeRedundantParentheses

            // if (stream.Position + dataSize > stream.Length)
            // {
            //     
            // }

            if (!stream.GetStructs<DataFileItemType>(header.ItemTypesCount, out var itemTypes))
            {
                error = "Get item types error";
                return false;
            }
            
            error = string.Empty;
            return true;
        }
    }
}