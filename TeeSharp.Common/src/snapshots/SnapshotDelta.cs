using System;
using TeeSharp.Common.Protocol;

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

            // pack deleted stuff
            for (var i = 0; i < from.ItemsCount; i++)
            {
                var fromItem = from[i];
                if (GetItemIndexHashed(fromItem.Key, hashItems) == -1)
                {
                    // deleted
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
                    var itemDataOffset = outputOffset + 3;
                    var pastItem = from[pastIndex];

                    if (DiffItem(pastItem.Object, currentItem.Object,
                            outputData, outputOffset) != 0)
                    {
                        outputData[outputOffset++] = currentItem.Type;
                        outputData[outputOffset++] = currentItem.Id;
                        outputData[outputOffset++] = currentItem.Object.FieldsCount;

                        outputOffset += currentItem.Object.FieldsCount;
                        numUpdatedItems++;
                    }

                }
                else
                {
                    outputData[outputOffset++] = currentItem.Type;
                    outputData[outputOffset++] = currentItem.Id;
                    outputData[outputOffset++] = currentItem.Object.FieldsCount;

                    Array.Copy(currentItem.Object.Serialize(), 0, 
                        outputData, outputOffset, currentItem.Object.FieldsCount);

                    outputOffset += currentItem.Object.FieldsCount;
                    numUpdatedItems++;
                }
            }

            if (numDeletedItems == 0 && numUpdatedItems == 0 && numTempItems == 0)
                return 0;

            return outputOffset;
        }

        public static int DiffItem(BaseSnapObject past, BaseSnapObject current,
            int[] outputData, int outputOffset)
        {
            var needed = 0;
            var pastData = past.Serialize();
            var currentdata = current.Serialize();

            for (int i = 0; i < current.FieldsCount; i++)
            {
                var @out = currentdata[i] - pastData[i];
                needed |= @out;
                outputData[outputOffset++] = @out;
            }

            return needed;
        }

        public static int GetItemIndexHashed(int key, HashItem[] hashItems)
        {
            var hashId = ((key >> 12) & 0b1111_0000) | (key & 0b1111);
            for (var i = 0; i < hashItems[hashId].Num; i++)
            {
                if (hashItems[hashId].Keys[i] == key)
                    return hashItems[hashId].Indexes[i];
            }

            return -1;
        }

        public static void GenerateHash(HashItem[] hashItems, Snapshot snapshot)
        {
            for (var i = 0; i < hashItems.Length; i++)
                hashItems[i].Num = 0;

            for (var i = 0; i < snapshot.ItemsCount; i++)
            {
                var key = snapshot[i].Key;
                var hashId = ((key >> 12) & 0b1111_0000) | (key & 0b1111);

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