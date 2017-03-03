using System;
using System.Collections.Generic;

namespace Teecsharp
{
    public class CSnapshotItem
    {
        public int Type() { return m_TypeAndID >> 16; }
        public int ID() { return m_TypeAndID & 0xffff; }
        public int Key() { return m_TypeAndID; }

        public int m_ObjectSize;
        public int m_TypeAndID;
        public CNet_Common m_Object;
    }

    public class CSnapshot
    {
        private int m_DataSize;
        private CSnapshotItem[] m_Data;

        public int NumItems()
        {
            return m_Data.Length;
        }

        public int DataSize()
        {
            return m_DataSize;
        }

        public void SetData(int dataSize, CSnapshotItem[] data)
        {
            m_DataSize = dataSize;
            m_Data = data;
        }

        public CSnapshotItem GetItem(int index)
        {
            return m_Data[index];
        }

        public CSnapshot()
        {
            m_Data = new CSnapshotItem[0];
        }

        public void Clear()
        {
            m_DataSize = 0;
            m_Data = new CSnapshotItem[0];
        }

        public int Crc()
        {
            int crc = 0;
            if (m_Data != null)
            {
                for (int i = 0; i < m_Data.Length; i++)
                {
                    var arr = m_Data[i].m_Object.GetArray();
                    for (int j = 0; j < arr.Length; j++)
                        crc += arr[j];
                }
            }
            return crc;
        }
    }

    public class CItemList
    {
        public int m_Num;
        public int[] m_aKeys;
        public int[] m_aIndex;

        public CItemList()
        {
            m_aKeys = new int[64];
            m_aIndex = new int[64];
        }
    }

    public class CSnapshotDelta
    {
        private const int HASHLIST_SIZE = 256;

        private static void GenerateHash(CItemList[] pHashlist, CSnapshot pSnapshot)
        {
            for (int i = 0; i < HASHLIST_SIZE; i++)
                pHashlist[i].m_Num = 0;

            for (int i = 0; i < pSnapshot.NumItems(); i++)
            {
                int Key = pSnapshot.GetItem(i).Key();
                int HashID = ((Key >> 12) & 0xf0) | (Key & 0xf);
                if (pHashlist[HashID].m_Num != 64)
                {
                    pHashlist[HashID].m_aIndex[pHashlist[HashID].m_Num] = i;
                    pHashlist[HashID].m_aKeys[pHashlist[HashID].m_Num] = Key;
                    pHashlist[HashID].m_Num++;
                }
            }
        }
        
        private static int GetItemIndexHashed(int Key, CItemList[] pHashlist)
        {
            int HashID = ((Key >> 12) & 0xf0) | (Key & 0xf);
	        for(int i = 0; i < pHashlist[HashID].m_Num; i++)
	        {
		        if(pHashlist[HashID].m_aKeys[i] == Key)
			        return pHashlist[HashID].m_aIndex[i];
	        }

	        return -1;
        }

        private static int DiffItem(CSnapshotItem pPast, CSnapshotItem pCurrent, int[] pDst, int pDstIndex)
        {
            int Needed = 0;
            var pCurrentArr = pCurrent.m_Object.GetArray();
            var pPastArr = pPast.m_Object.GetArray();

            for (int i = 0; i < pCurrentArr.Length; i++)
            {
                int pOut = pCurrentArr[i] - pPastArr[i];
                Needed |= pOut;
                pDst[pDstIndex++] = pOut;
            }
            return Needed;
        }

        public int CreateDelta(CSnapshot pFrom, CSnapshot pTo, int[] pDstData)
        {
            CSnapshotItem pFromItem;
            CSnapshotItem pCurItem;
            CSnapshotItem pPastItem;

            int pDstDataIndex = 3;
            int m_NumDeletedItems = 0;
            int m_NumUpdateItems = 0;
            int m_NumTempItems = 0;

            CItemList[] Hashlist = new CItemList[HASHLIST_SIZE];
            for (int i = 0; i < HASHLIST_SIZE; i++)
                Hashlist[i] = new CItemList();
            GenerateHash(Hashlist, pTo);

            // pack deleted stuff
            for (int i = 0; i < pFrom.NumItems(); i++)
            {
                pFromItem = pFrom.GetItem(i);
                if (GetItemIndexHashed(pFromItem.Key(), Hashlist) == -1)
                {
                    // deleted
                    m_NumDeletedItems++;
                    pDstData[pDstDataIndex++] = pFromItem.Key();
                }
            }

            GenerateHash(Hashlist, pFrom);
            int[] aPastIndecies = new int[CSnapshotBuilder.MAX_ITEMS];

            // fetch previous indices
            // we do this as a separate pass because it helps the cache
            int NumItems = pTo.NumItems();
            for (int i = 0; i < NumItems; i++)
            {
                pCurItem = pTo.GetItem(i); // O(1) .. O(n)
                aPastIndecies[i] = GetItemIndexHashed(pCurItem.Key(), Hashlist); // O(n) .. O(n^n)
            }

            for (int i = 0; i < NumItems; i++)
            {
                // do delta
                pCurItem = pTo.GetItem(i); // O(1) .. O(n)
                int PastIndex = aPastIndecies[i];

                if (PastIndex != -1)
                {
                    int pItemDataDst = pDstDataIndex + 3;
                    pPastItem = pFrom.GetItem(PastIndex);

                    if (pCurItem.m_ObjectSize != 0)
                        pItemDataDst = pDstDataIndex + 2;

                    if (DiffItem(pPastItem, pCurItem, pDstData, pItemDataDst) != 0)
                    {
                        pDstData[pDstDataIndex++] = pCurItem.Type();
                        pDstData[pDstDataIndex++] = pCurItem.ID();

                        if (pCurItem.m_ObjectSize == 0)
                            pDstData[pDstDataIndex++] = pCurItem.m_ObjectSize/ sizeof(int);

                        pDstDataIndex += pCurItem.m_ObjectSize/ sizeof(int);
                        m_NumUpdateItems++;
                    }
                }
                else
                {
                    pDstData[pDstDataIndex++] = pCurItem.Type();
                    pDstData[pDstDataIndex++] = pCurItem.ID();

                    if (pCurItem.m_ObjectSize == 0)
                        pDstData[pDstDataIndex++] = pCurItem.m_ObjectSize / sizeof(int);

                    var arr = pCurItem.m_Object.GetArray();
                    Buffer.BlockCopy(arr, 0, pDstData, pDstDataIndex * sizeof(int), arr.Length * sizeof(int));

                    pDstDataIndex += arr.Length;
                    m_NumUpdateItems++;
                }
            }

            if (m_NumDeletedItems == 0 && m_NumUpdateItems == 0 && m_NumTempItems == 0)
                return 0;

            pDstData[0] = m_NumDeletedItems;
            pDstData[1] = m_NumUpdateItems;
            pDstData[2] = m_NumTempItems;

            return pDstDataIndex;
        }
    }

    public class CSnapshotStorage
    {
        private class CHolder
        {
            public long m_Tagtime;
            public int m_Tick;
            public int m_SnapSize;
            public CSnapshot m_pSnap;
        }

        private readonly List<CHolder> m_HolderList;

        public CSnapshotStorage()
        {
            m_HolderList = new List<CHolder>();
        }

        public void Init()
        {
            m_HolderList.Clear();
        }

        public void PurgeAll()
        {
            m_HolderList.Clear();
        }

        public void PurgeUntil(int Tick)
        {
            for (int i = 0; i < m_HolderList.Count; i++)
            {
                if (m_HolderList[i].m_Tick >= Tick)
                    return; // no more to remove

                m_HolderList.RemoveAt(i);
                i--;
            }
            m_HolderList.Clear();
        }

        public void Add(int Tick, long Tagtime, int DataSize, CSnapshot pData)
        {
            // set data
            CHolder pHolder = new CHolder();
            pHolder.m_Tick = Tick;
            pHolder.m_Tagtime = Tagtime;
            pHolder.m_SnapSize = DataSize;
            pHolder.m_pSnap = pData;

            // link
            m_HolderList.Add(pHolder);
        }

        public int Get(int Tick, ref long pTagtime, ref CSnapshot ppData)
        {
            for (int i = 0; i < m_HolderList.Count; i++)
            {
                if (m_HolderList[i].m_Tick == Tick)
                {
                    pTagtime = m_HolderList[i].m_Tagtime;
                    ppData = m_HolderList[i].m_pSnap;
                    return m_HolderList[i].m_SnapSize;
                }
            }
            return -1;
        }
    }

    public class CSnapshotBuilder
    {
        public const int MAX_SIZE = 65536;
        public const int MAX_ITEMS = 1024;

        //private int m_DataSize;
        private readonly List<CSnapshotItem> m_CurrentSnapshotData;
        private int m_CurrentSnapshotSize;

        public CSnapshotBuilder()
        {
            m_CurrentSnapshotData = new List<CSnapshotItem>();
            m_CurrentSnapshotSize = 0;
        }

        ~CSnapshotBuilder()
        {
            m_CurrentSnapshotData.Clear();
        }

        public void Init()
        {
            m_CurrentSnapshotData.Clear();
            m_CurrentSnapshotSize = 0;
        }

        public CSnapshot Finish()
        {
            CSnapshot newSnapshot = new CSnapshot();
            newSnapshot.SetData(m_CurrentSnapshotSize, m_CurrentSnapshotData.ToArray());
            return newSnapshot;
        }

        public object NewEvent(Type Type, int ID, int TypeID)
        {
            int size = CSystem.get_cached_fields(Type).CachedSize;

            if (m_CurrentSnapshotSize + size >= MAX_SIZE)
            {
                CSystem.dbg_msg("snapshots", "too much data");
                return null;
            }
            if (m_CurrentSnapshotData.Count + 1 >= MAX_ITEMS)
            {
                CSystem.dbg_msg("snapshots", "too many items");
                return null;
            }

            CSnapshotItem item = new CSnapshotItem();
            item.m_TypeAndID = (TypeID << 16) | ID;
            item.m_Object = (CNetEvent_Common)Activator.CreateInstance(Type);
            item.m_ObjectSize = size;

            m_CurrentSnapshotData.Add(item);
            m_CurrentSnapshotSize += size;

            return item.m_Object;
        }

        public T NewNetObj<T>(int ID, int Type) where T : CNet_Common, new ()
        {
            int size = CSystem.get_cached_fields(typeof(T)).CachedSize;

            if (m_CurrentSnapshotSize + size >= MAX_SIZE)
            {
                CSystem.dbg_msg("snapshots", "too much data");
                return null;
            }
            if (m_CurrentSnapshotData.Count + 1 >= MAX_ITEMS)
            {
                CSystem.dbg_msg("snapshots", "too many items");
                return null;
            }

            CSnapshotItem item = new CSnapshotItem();
            item.m_TypeAndID = (Type << 16) | ID;
            item.m_Object = new T();
            item.m_ObjectSize = size;

            m_CurrentSnapshotData.Add(item);
            m_CurrentSnapshotSize += size;

            return (T)item.m_Object;
        }
    }
}
