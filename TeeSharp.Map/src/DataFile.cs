using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using TeeSharp.Core.Extensions;
using TeeSharp.Core.Helpers;
using TeeSharp.Map.Abstract;
using TeeSharp.Map.DataFileItems;
using TeeSharp.Map.MapItems;

namespace TeeSharp.Map;

public class DataFile
{
    public readonly ReadOnlyMemory<byte> Buffer;
    public readonly DataFileHeader Header;
    public readonly ReadOnlyDictionary<MapItemType, DataFileItemTypeInfo> ItemTypes;
    public readonly ReadOnlyCollection<int> ItemsOffsets;
    public readonly ReadOnlyCollection<int> DataOffsets;
    public readonly ReadOnlyCollection<int> DataSizes;
    public readonly int ItemsStartOffset;
    public readonly int DataStartOffset;

    protected Dictionary<int, object> DataItems { get; set; }

    public DataFile(
        ReadOnlyMemory<byte> buffer,
        DataFileHeader header,
        IEnumerable<DataFileItemTypeInfo> itemTypes,
        int[] itemsOffsets,
        int[] dataOffsets,
        int[] dataSizes,
        int itemsStartOffset,
        int dataStartOffset)
    {
        Buffer = buffer;
        Header = header;
        ItemTypes = new ReadOnlyDictionary<MapItemType, DataFileItemTypeInfo>(itemTypes.ToDictionary(info => info.Type));
        ItemsOffsets = Array.AsReadOnly(itemsOffsets);
        DataOffsets = Array.AsReadOnly(dataOffsets);
        DataSizes = Array.AsReadOnly(dataSizes);
        ItemsStartOffset = itemsStartOffset;
        DataStartOffset = dataStartOffset;
        DataItems = new Dictionary<int, object>();
    }

    public bool HasItemType(MapItemType type)
    {
        return ItemTypes.ContainsKey(type);
    }

    public DataFileItemTypeInfo GetItemType(MapItemType type)
    {
        return ItemTypes[type];
    }

    public IEnumerable<(DataFileItem Info, T Item)> GetItems<T>(MapItemType type)
        where T : struct, IDataFileItem
    {
        var itemTypeInfo = GetItemType(type);
        for (var i = 0; i < itemTypeInfo.ItemsCount; i++)
            yield return GetItem<T>(itemTypeInfo.ItemsOffset + i);
    }

    public (DataFileItem Info, T Item) GetItem<T>(int index)
        where T : struct, IDataFileItem
    {
        // TODO
        // add external item types support from DDNet

        var offset = ItemsStartOffset + ItemsOffsets[index];
        var buffer = Buffer.Span.Slice(offset);
        var itemInfo = buffer.Deserialize<DataFileItem>();

        buffer = buffer.Slice(StructHelper<DataFileItem>.Size, itemInfo.Size);
        T item;

        if (StructHelper<T>.Size == itemInfo.Size)
        {
           item = buffer.Deserialize<T>();
        }
        else
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                throw new Exception("IsReferenceOrContainsReferences");

            if (buffer.Length > StructHelper<T>.Size)
                throw new ArgumentOutOfRangeException(nameof(T));

            item = new T();
            ref var itemRef =
                ref Unsafe.As<T, byte>(ref item);
            ref var bufferRef =
                ref MemoryMarshal.GetReference(buffer);

            Unsafe.CopyBlockUnaligned(ref itemRef, ref bufferRef, (uint)buffer.Length);
        }

        return (itemInfo, item);
    }

    public ReadOnlySpan<byte> GetDataAsRaw(int index)
    {
        return GetDataBuffer(index);
    }

    public string GetDataAsString(int index)
    {
        if (DataItems.TryGetValue(index, out var data))
            return (string) data;

        var buffer = GetDataBuffer(index);
        if (buffer[^1] == 0)
            buffer = buffer[..^1];

        DataItems.Add(index, Encoding.UTF8.GetString(buffer));
        return (string) DataItems[index];
    }

    public T GetDataAs<T>(int index) where T : struct, IDataFileItem
    {
        if (DataItems.TryGetValue(index, out var data))
            return (T) data;

        var buffer = GetDataBuffer(index);
        DataItems.Add(index, buffer.Deserialize<T>());

        return (T) DataItems[index];
    }

    public T[] GetDataAsArrayOf<T>(int index) where T : struct, IDataFileItem
    {
        if (DataItems.TryGetValue(index, out var data))
            return (T[]) data;

        var buffer = GetDataBuffer(index);
        DataItems.Add(index, buffer.Deserialize<T>(buffer.Length / StructHelper<T>.Size).ToArray());

        return (T[]) DataItems[index];
    }

    private ReadOnlySpan<byte> GetDataBuffer(int index)
    {
        var dataSize = index == Header.NumberOfRawDataBlocks - 1
            ? Header.RawDataBlocksSize - DataOffsets[index]
            : DataOffsets[index + 1] - DataOffsets[index];

        var dataOffset = DataStartOffset + DataOffsets[index];
        var data = Buffer.Span.Slice(dataOffset, dataSize).ToArray();

        using var outputStream = new MemoryStream();
        using var compressedStream = new MemoryStream(data);
        using var inflaterInputStream = new InflaterInputStream(compressedStream);

        inflaterInputStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }

    public void UnloadData(int index)
    {
        DataItems.Remove(index);
    }
}
