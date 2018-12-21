using System;
using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Snapshots
{
    public static class SnapshotDelta
    {
        private const int HASHLIST_SIZE = 256;

        public class HashItem
        {
            public int Num;
            public readonly int[] Keys;
            public readonly int[] Indexes;

            public HashItem()
            {
                Keys = new int[64];
                Indexes = new int[64];
            }
        }

        public static Snapshot UnpackDelta(Snapshot from, int[] inputData, 
            int inputOffset, int inputSize)
        {
            var snapshotBuilder = new SnapshotBuilder();
            var endIndex = inputOffset + inputSize;

            var numDeletedItems = inputData[inputOffset++];
            var numUpdatedItems = inputData[inputOffset++];
            var numTempItems = inputData[inputOffset++];
            var deletedOffset = inputOffset;

            inputOffset += numDeletedItems;

            if (inputOffset > endIndex)
                return null;
            
            snapshotBuilder.StartBuild();

            for (var i = 0; i < from.ItemsCount; i++)
            {
                var item = from[i];
                var keep = true;

                for (var d = 0; d < numDeletedItems; d++)
                {
                    if (inputData[deletedOffset + d] == item.Key)
                    {
                        keep = false;
                        break;
                    }
                }

                if (keep)
                {
                    snapshotBuilder.AddItem(item.Item.MakeCopy(), item.Id);
                }
            }

            for (var i = 0; i < numUpdatedItems; i++)
            {
                if (inputOffset + 2 > endIndex)
                    return null;

                var type = (SnapshotItems) inputData[inputOffset++];
                var id = inputData[inputOffset++];
                int itemSize; // in bytes

                if (SnapshotItemsInfo.GetSizeByType(type) != 0)
                    itemSize = SnapshotItemsInfo.GetSizeByType(type);
                else
                {
                    if (inputOffset + 1 > endIndex)
                        return null;

                    itemSize = inputData[inputOffset++] * sizeof(int);
                }

                if (itemSize < 0 || !RangeCheck(endIndex, inputOffset, itemSize / sizeof(int)))
                    return null;

                var key = ((int) type << 16) | id;
                var newItem = snapshotBuilder.FindItem(key)?.Item;

                if (newItem == null)
                {
                    var item = SnapshotItemsInfo.GetInstanceByType(type);
                    if (snapshotBuilder.AddItem(item, id))
                        newItem = item;
                }

                if (newItem == null)
                    return null;

                var fromItem = from.FindItem(key);
                if (fromItem != null)
                {
                    UndiffItem(fromItem.Item, inputData, inputOffset, newItem);
                }
                else
                {
                    newItem.Deserialize(inputData, inputOffset);
                }

                inputOffset += itemSize / sizeof(int);
            }

            return snapshotBuilder.EndBuild();
        }

        public static bool RangeCheck(int endIndex, int currentIndex, int size)
        {
            return currentIndex + size <= endIndex;
        }

        public static int CreateDelta(Snapshot from, Snapshot to, int[] outputData)
        {
            var numDeletedItems = 0;
            var numUpdatedItems = 0;
            var numTempItems = 0;
            var outputOffset = 3;

            var hashItems = new HashItem[HASHLIST_SIZE];
            for (var i = 0; i < hashItems.Length; i++)
                hashItems[i] = new HashItem();

            GenerateHash(hashItems, to);

            for (var i = 0; i < from.ItemsCount; i++)
            {
                var fromItem = from[i];
                if (GetItemIndexHashed(fromItem.Key, hashItems) == -1)
                {
                    numDeletedItems++;
                    outputData[outputOffset++] = fromItem.Key;
                }
            }

            GenerateHash(hashItems, from);
            var pastIndecies = new int[SnapshotBuilder.MAX_SNAPSHOT_ITEMS];

            // fetch previous indices
            // we do this as a separate pass because it helps the cache
            for (var i = 0; i < to.ItemsCount; i++)
                pastIndecies[i] = GetItemIndexHashed(to[i].Key, hashItems);

            for (var i = 0; i < to.ItemsCount; i++)
            {
                var currentItem = to[i];
                var pastIndex = pastIndecies[i];
                
                if (pastIndex != -1)
                {
                    var pastItem = from[pastIndex];
                    var offset = outputOffset + 3;

                    if (SnapshotItemsInfo.GetSizeByType(currentItem.Type) != 0)
                        offset = outputOffset + 2;

                    if (DiffItem(pastItem.Item, currentItem.Item,
                            outputData, offset) != 0)
                    {
                        outputData[outputOffset++] = (int) currentItem.Type;
                        outputData[outputOffset++] = currentItem.Id;

                        if (SnapshotItemsInfo.GetSizeByType(currentItem.Type) == 0)
                            outputData[outputOffset++] = currentItem.Item.SerializeLength;

                        outputOffset += currentItem.Item.SerializeLength; // count item int fields
                        numUpdatedItems++;
                    }
                }
                else
                {
                    outputData[outputOffset++] = (int) currentItem.Type;
                    outputData[outputOffset++] = currentItem.Id;

                    if (SnapshotItemsInfo.GetSizeByType(currentItem.Type) == 0)
                        outputData[outputOffset++] = currentItem.Item.SerializeLength;

                    var data = currentItem.Item.Serialize();
                    Array.Copy(data, 0, outputData, outputOffset, data.Length);

                    outputOffset += data.Length;
                    numUpdatedItems++;
                }
            }

            if (numDeletedItems == 0 && numUpdatedItems == 0 && numTempItems == 0)
                return 0;

            outputData[0] = numDeletedItems;
            outputData[1] = numUpdatedItems;
            outputData[2] = numTempItems;

            return outputOffset;
        }

        private static void UndiffItem(BaseSnapshotItem past, int[] inputData, 
            int inputOffset, BaseSnapshotItem newItem)
        {
            var pastData = past.Serialize();
            var newData = new int[pastData.Length];

            for (int i = 0; i < past.SerializeLength; i++)
            {
                newData[i] = pastData[i] + inputData[inputOffset + i];
            }

            newItem.Deserialize(newData, 0);
        }

        private static int DiffItem(BaseSnapshotItem past, BaseSnapshotItem current,
            int[] outputData, int outputOffset)
        {
            var needed = 0;
            var pastData = past.Serialize();
            var currentdata = current.Serialize();

            for (int i = 0; i < current.SerializeLength; i++)
            {
                var @out = currentdata[i] - pastData[i];
                needed |= @out;
                outputData[outputOffset++] = @out;
            }

            return needed;
        }

        private static int GetItemIndexHashed(int key, HashItem[] hashItems)
        {
            var hashId = ((key >> 12) & 0xF0) | (key & 0xF);
            for (var i = 0; i < hashItems[hashId].Num; i++)
            {
                if (hashItems[hashId].Keys[i] == key)
                    return hashItems[hashId].Indexes[i];
            }

            return -1;
        }

        private static void GenerateHash(HashItem[] hashItems, Snapshot snapshot)
        {
            for (var i = 0; i < hashItems.Length; i++)
                hashItems[i].Num = 0;

            for (var i = 0; i < snapshot.ItemsCount; i++)
            {
                var key = snapshot[i].Key;
                var hashId = ((key >> 12) & 0xF0) | (key & 0xF);

                if (hashItems[hashId].Num != 64)
                {
                    hashItems[hashId].Indexes[hashItems[hashId].Num] = i;
                    hashItems[hashId].Keys[hashItems[hashId].Num] = key;
                    hashItems[hashId].Num++;
                }
            }
        }
    }
}