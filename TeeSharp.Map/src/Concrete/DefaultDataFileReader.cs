using System;
using System.IO;
using TeeSharp.Core.Extensions;
using TeeSharp.Core.Helpers;
using TeeSharp.Map.Abstract;
using TeeSharp.Map.DataFileItems;
using TeeSharp.Map.MapItems;

namespace TeeSharp.Map.Concrete;

public class DefaultDataFileReader : IDataFileReader
{
    public static readonly IDataFileReader Instance = new DefaultDataFileReader();

    public DataFile Read(string path)
    {
        using var stream = File.OpenRead(path);
        return Read(stream);
    }

    public DataFile Read(Stream stream)
    {
        stream.Position = 0;

        if (stream.TryRead<DataFileHeader>(out var header))
        {
            if (!header.IsValidSignature)
                throw new Exception("Wrong map header signature");
        }
        else
        {
            throw new Exception("Parse map header error");
        }

        if (!header.IsValidVersion)
            throw new Exception($"Wrong map version ({header.Version})");

        // ReSharper disable ArrangeRedundantParentheses
        var fileSize =
            (StructHelper<DataFileHeader>.Size) +
            (header.NumberOfItemTypes * StructHelper<DataFileItemTypeInfo>.Size) +
            (header.NumberOfItems + header.NumberOfRawDataBlocks + header.NumberOfRawDataBlocks) * sizeof(int) +
            (header.ItemsSize) +
            (header.RawDataBlocksSize);
        // ReSharper restore ArrangeRedundantParentheses

        if (fileSize != stream.Length)
            throw new Exception("Invalid file size");

        if (!stream.TryRead<DataFileItemTypeInfo>(header.NumberOfItemTypes, out var itemTypes))
            throw new Exception("Get map item types error");

        if (!stream.TryRead<int>(header.NumberOfItems, out var itemsOffsets))
            throw new Exception("Get map items offsets error");

        if (!stream.TryRead<int>(header.NumberOfRawDataBlocks, out var dataOffsets))
            throw new Exception("Get map data offsets error");

        if (!stream.TryRead<int>(header.NumberOfRawDataBlocks, out var dataSizes))
            throw new Exception("Get map data offsets error");

        using var bufferStream = new MemoryStream((int)stream.Length);
        stream.Position = 0;
        stream.CopyTo(bufferStream);

        return new DataFile(
            buffer: bufferStream.ToArray(),
            header: header,
            itemTypes: itemTypes,
            itemsOffsets: itemsOffsets,
            dataOffsets: dataOffsets,
            dataSizes: dataSizes,
            itemsStartOffset: stream.Position,
            dataStartOffset: stream.Position + header.ItemsSize
        );
    }
}
