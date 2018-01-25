using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ComponentAce.Compression.Libs.zlib;
using TeeSharp.Core;

namespace TeeSharp.Map
{
    public class DataFile
    {
        public byte[] Raw;
        public readonly uint Crc;
        public readonly DataFileVersionHeader VersionHeader;
        public readonly DataFileHeader Header;
        public readonly DataFileItemType[] ItemTypes;
        public readonly int[] ItemOffsets;
        public readonly int[] DataOffsets;
        public readonly int[] DataSizes;

        public int ItemStartIndex { get; }
        public int DataStartIndex => ItemStartIndex + Header.ItemSize;

        private readonly object[] _dataObjects;

        public DataFile(
            byte[] raw,
            uint crc, 
            DataFileVersionHeader versionHeader,
            DataFileHeader header, 
            DataFileItemType[] itemTypes,
            int[] itemOffsets,
            int[] dataOffsets,
            int[] dataSizes,
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
            if (_dataObjects[index] == null)
            {
                var dataSize = GetDataSize(index);
                var uncompressedSize = DataSizes[index];

                Debug.Log("datafile", $"loading data index={index} size={dataSize} uncompressed={uncompressedSize}");
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
                    _dataObjects[index] = outMemoryStream.ToArray().ReadStruct<T>();
                }
            }

            return (T) _dataObjects[index];
        }

        public void GetType(int typeId, out int start, out int num)
        {
            for (var i = 0; i < Header.NumItemTypes; i++)
            {
                if (ItemTypes[i].TypeId == typeId)
                {
                    start = ItemTypes[i].Start;
                    num = ItemTypes[i].Num;
                    return;
                }
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
                var item = GetItem<T>(start + i, out var _, out var itemId);
                if (id == itemId)
                    return item;
            }

            return default(T);
        }
    }
}