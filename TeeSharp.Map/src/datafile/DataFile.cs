using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ComponentAce.Compression.Libs.zlib;
using TeeSharp.Core;

namespace TeeSharp.Map
{
    public class DataFile
    {
        public readonly byte[] Raw;
        public readonly uint Crc;
        public readonly DataFileVersionHeader VersionHeader;
        public readonly DataFileHeader Header;
        public readonly IReadOnlyList<DataFileItemType> ItemTypes;
        public readonly IReadOnlyList<int> ItemOffsets;
        public readonly IReadOnlyList<int> DataOffsets;
        public readonly IReadOnlyList<int> DataSizes;

        public int ItemStartIndex { get; }
        public int DataStartIndex => ItemStartIndex + Header.ItemSize;

        private readonly object[] _dataObjects;

        public DataFile(
            byte[] raw,
            uint crc, 
            DataFileVersionHeader versionHeader,
            DataFileHeader header, 
            IReadOnlyList<DataFileItemType> itemTypes,
            IReadOnlyList<int> itemOffsets,
            IReadOnlyList<int> dataOffsets,
            IReadOnlyList<int> dataSizes,
            int itemStartIndex)
        {
            Raw = raw;
            Crc = crc;
            VersionHeader = versionHeader;
            Header = header;
            ItemTypes = itemTypes;
            ItemOffsets = itemOffsets;
            DataOffsets = dataOffsets;
            DataSizes = dataSizes;
            ItemStartIndex = itemStartIndex;

            _dataObjects = new object[Header.NumData];
        }

        // TODO
        public void UnloadData(int index)
        {
            _dataObjects[index] = null;
        }

        public int GetDataSize(int index)
        {
            if (index == Header.NumData - 1)
                return Header.DataSize - DataOffsets[index];
            return DataOffsets[index + 1] - DataOffsets[index];
        }
        
        public T GetData<T>(int index)
        {
            if (_dataObjects[index] != null)
                return (T) _dataObjects[index];

            var dataSize = GetDataSize(index);
            var uncompressedSize = DataSizes[index];

            Debug.Log("datafile", $"loading data={typeof(T).Name} index={index} size={dataSize} uncompressed={uncompressedSize}");
            using (var outMemoryStream = new MemoryStream())
            using (var outZStream = new ZOutputStream(outMemoryStream))
            using (var inMemoryStream = new MemoryStream(
                Raw,
                DataStartIndex + DataOffsets[index],
                dataSize
            ))
            {
                inMemoryStream.CopyStream(outZStream);
                outZStream.finish();

                // TODO
                if (typeof(T).IsArray)
                {
                    if (typeof(T) == typeof(string[]))
                        throw new NotSupportedException("GetData not supported array string");

                    _dataObjects[index] = outMemoryStream.ToArray().ReadStructs(typeof(T).GetElementType());
                }
                else
                {
                    if (typeof(T) == typeof(string))
                    {
                        _dataObjects[index] = Encoding.UTF8.GetString(outMemoryStream.ToArray()).SanitizeCC();
                    }
                    else
                        _dataObjects[index] = outMemoryStream.ToArray().ReadStruct<T>();
                }
            }

            return (T) _dataObjects[index];
        }

        public void GetType(int typeId, out int start, out int num)
        {
            for (var i = 0; i < Header.NumItemTypes; i++)
            {
                if (ItemTypes[i].TypeId != typeId)
                    continue;

                start = ItemTypes[i].Start;
                num = ItemTypes[i].Num;
                return;
            }

            start = 0;
            num = 0;
        }

        public T GetItem<T>(int index, out int typeId, out int id)
        {
            var offset = ItemStartIndex + ItemOffsets[index];
            var item = Raw.ReadStruct<DataFileItem>(offset);

            typeId = (item.TypeIdAndItemId >> 16) & 0b1111_1111_1111_1111;
            id = item.TypeIdAndItemId & 0b1111_1111_1111_1111;

            return Raw.ReadStruct<T>(offset + sizeof(int) * 2);
        }

        public T FindItem<T>(int typeId, int id)
        {
            GetType(typeId, out var start, out var num);

            for (var i = 0; i < num; i++)
            {
                var item = GetItem<T>(start + i, out _, out var itemId);
                if (id == itemId)
                    return item;
            }

            return default(T);
        }
    }
}