using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using zlib;

//using DamienG.Security.Cryptography;
//using TeeLibs;

namespace Teecsharp
{
    class CDatafileItemType
    {
        public int m_Type;
        public int m_Start;
        public int m_Num;
    }

    class CDatafileItem
    {
        public int m_TypeAndID;
        public int m_Size;
    }

    class CDatafileHeader
    {
        public readonly char[] m_aID = new char[4];
        public int m_Version;
        public int m_Size;
        public int m_Swaplen;
        public int m_NumItemTypes;
        public int m_NumItems;
        public int m_NumRawData;
        public int m_ItemSize;
        public int m_DataSize;
    }

    class CDatafileData
    {
        public int m_NumItemTypes;
        public int m_NumItems;
        public int m_NumRawData;
        public int m_ItemSize;
        public int m_DataSize;
        public readonly char[] m_aStart = new char[4];
    }


    struct CDatafileInfo
    {
        public CDatafileItemType[] m_pItemTypes;
        public int[] m_pItemOffsets;
        public int[] m_pDataOffsets;
        public int[] m_pDataSizes;

        public long m_pItemStart;
        public long m_pDataStart;
    }

    class CDatafile
    {
        public FileStream m_File;
        public uint m_Crc;
        public CDatafileInfo m_Info;
        public CDatafileHeader m_Header;
        public long m_DataStartOffset;
        public object[] m_ppDataObjects;
        public byte[] m_pData;
    }

    class CDataFileReader
    {
        private CDatafile m_pDataFile;

        public CDataFileReader()
        {
            m_pDataFile = null;
        }

        public FileStream GetStream()
        {
            return m_pDataFile.m_File;
        }

        public bool Open(IStorage pStorage, string pFilename, int StorageType)
        {
            CSystem.dbg_msg("datafile", "loading. filename='{0}'", pFilename);
            
            var fileStream = pStorage.OpenFile(pFilename, CSystem.IOFLAG_READ, StorageType);
            if (fileStream == null)
            {
                CSystem.dbg_msg("datafile", "could not open '{0}'", pFilename);
                return false;
            }

            var aBuffer = new byte[fileStream.Length];
            CSystem.io_read(fileStream, aBuffer, (int)fileStream.Length);
            CSystem.io_seek(fileStream, 0, SeekOrigin.Begin);
            var crc = Crc32.ComputeChecksum(aBuffer);

            // TODO: change this header
            CDatafileHeader datafileHeader;
            CSystem.read_obj_from_stream(out datafileHeader, fileStream);
            if (datafileHeader.m_aID[0] != 'A' || datafileHeader.m_aID[1] != 'T' || datafileHeader.m_aID[2] != 'A' || datafileHeader.m_aID[3] != 'D')
            {
                if (datafileHeader.m_aID[0] != 'D' || datafileHeader.m_aID[1] != 'A' || datafileHeader.m_aID[2] != 'T' || datafileHeader.m_aID[3] != 'A')
                {
                    CSystem.dbg_msg("datafile", "wrong signature. {0} {0} {0} {0}",
                        datafileHeader.m_aID[0], datafileHeader.m_aID[1], datafileHeader.m_aID[2], datafileHeader.m_aID[3]);
                    return false;
                }
            }

            if (datafileHeader.m_Version != 4)
            {
                CSystem.dbg_msg("datafile", "wrong version. version={0}", datafileHeader.m_Version);
                return false;
            }

            // read in the rest except the data
            long Size = 0;
            Size += datafileHeader.m_NumItemTypes*sizeof (int)*3;//Marshal.SizeOf<CDatafileItemType>();
            Size += (datafileHeader.m_NumItems + datafileHeader.m_NumRawData) * sizeof(int);
            if (datafileHeader.m_Version == 4)
                Size += datafileHeader.m_NumRawData * sizeof(int); // v4 has uncompressed data sizes aswell
            Size += datafileHeader.m_ItemSize;

            //long AllocSize = Size;
            //AllocSize += Marshal.SizeOf<CDatafile>(); // add space for info structure
            //AllocSize += Header.m_NumRawData*Marshal.SizeOf<IntPtr>(); // add space for data pointers

            CDatafile pTmpDataFile = new CDatafile();
            pTmpDataFile.m_Header = datafileHeader;
            pTmpDataFile.m_pData = new byte[Size];
            pTmpDataFile.m_File = fileStream;
            pTmpDataFile.m_Crc = crc;
            pTmpDataFile.m_DataStartOffset = fileStream.Position + Size;
            pTmpDataFile.m_ppDataObjects = new object[datafileHeader.m_NumRawData];

            // read types, offsets, sizes and item data
            long fSeekPos = fileStream.Position;
            int ReadSize = CSystem.io_read(fileStream, pTmpDataFile.m_pData, pTmpDataFile.m_pData.Length);
            if (ReadSize != Size)
            {
                CSystem.io_close(pTmpDataFile.m_File);
                CSystem.dbg_msg("datafile", "couldn't load the whole thing, wanted={0} got={1}", Size, ReadSize);
                return false;
            }

            Close();
            m_pDataFile = pTmpDataFile;

            //if(DEBUG)
            {
                //CSystem.dbg_msg("datafile", "allocsize=%d", AllocSize);
                CSystem.dbg_msg("datafile", "readsize={0}", ReadSize);
                CSystem.dbg_msg("datafile", "swaplen={0}", datafileHeader.m_Swaplen);
                CSystem.dbg_msg("datafile", "item_size={0}", m_pDataFile.m_Header.m_ItemSize);
            }

            CSystem.io_seek(fileStream, fSeekPos, SeekOrigin.Begin);

            // read item types
            m_pDataFile.m_Info.m_pItemTypes = new CDatafileItemType[m_pDataFile.m_Header.m_NumItemTypes];
            for (int i = 0; i < m_pDataFile.m_Header.m_NumItemTypes; i++)
                CSystem.read_obj_from_stream(out m_pDataFile.m_Info.m_pItemTypes[i], fileStream);

            // read item offsets
            m_pDataFile.m_Info.m_pItemOffsets = new int[m_pDataFile.m_Header.m_NumItems];
            for (int i = 0; i < m_pDataFile.m_Header.m_NumItems; i++)
                CSystem.read_int_from_stream(out m_pDataFile.m_Info.m_pItemOffsets[i], fileStream);

            // read data offsets
            m_pDataFile.m_Info.m_pDataOffsets = new int[m_pDataFile.m_Header.m_NumRawData];
            for (int i = 0; i < m_pDataFile.m_Header.m_NumRawData; i++)
                CSystem.read_int_from_stream(out m_pDataFile.m_Info.m_pDataOffsets[i], fileStream);

            // read data sizes
            m_pDataFile.m_Info.m_pDataSizes = new int[m_pDataFile.m_Header.m_NumRawData];
            for (int i = 0; i < m_pDataFile.m_Header.m_NumRawData; i++)
                CSystem.read_int_from_stream(out m_pDataFile.m_Info.m_pDataSizes[i], fileStream);

            if (datafileHeader.m_Version == 4)
                m_pDataFile.m_Info.m_pItemStart = fileStream.Position;
            m_pDataFile.m_Info.m_pDataStart = m_pDataFile.m_Info.m_pItemStart + m_pDataFile.m_Header.m_ItemSize;

            CSystem.dbg_msg("datafile", "loading done. datafile='{0}'", pFilename);
            return true;
        }

        public int NumData()
        {
            if (m_pDataFile == null)
                return 0;
            return m_pDataFile.m_Header.m_NumRawData;
        }

        // always returns the size in the file
        public int GetDataSize(int Index)
        {
            if (m_pDataFile == null)
                return 0;

            if (Index == m_pDataFile.m_Header.m_NumRawData - 1)
                return m_pDataFile.m_Header.m_DataSize - m_pDataFile.m_Info.m_pDataOffsets[Index];
            return m_pDataFile.m_Info.m_pDataOffsets[Index + 1] - m_pDataFile.m_Info.m_pDataOffsets[Index];
        }

        public List<T> GetDataImpl<T>(int Index) where T : class, new()
        {
            if (m_pDataFile == null)
                return null;
            
            // load it if needed
            if (m_pDataFile.m_ppDataObjects[Index] == null)
            {
                // fetch the data size
                int DataSize = GetDataSize(Index);

                if (m_pDataFile.m_Header.m_Version == 4)
                {
                    int UncompressedSize = m_pDataFile.m_Info.m_pDataSizes[Index];
                    m_pDataFile.m_ppDataObjects[Index] = new List<T>();
                    CSystem.dbg_msg("datafile", "loading data index={0} size={1} uncompressed={2}", Index, DataSize, UncompressedSize);

                    // read the compressed data 1889955
                    byte[] buffer = new byte[DataSize];
                    CSystem.io_seek(m_pDataFile.m_File, m_pDataFile.m_DataStartOffset + m_pDataFile.m_Info.m_pDataOffsets[Index], SeekOrigin.Begin);
                    CSystem.io_read(m_pDataFile.m_File, buffer, DataSize);

                    // decompress the data
                    using (MemoryStream outMemoryStream = new MemoryStream())
                    using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
                    using (Stream inMemoryStream = new MemoryStream(buffer))
                    {
                        CSystem.copy_stream(inMemoryStream, outZStream);
                        outZStream.finish();

                        var sizeOfT = CSystem.get_cached_fields(typeof (T)).CachedSize;
                        outMemoryStream.Seek(0, SeekOrigin.Begin);
                        for (int i = 0; i < outMemoryStream.Length/sizeOfT; i++)
                        {
                            T obj = new T();
                            CSystem.read_obj_from_stream(out obj, outMemoryStream);
                            ((List<T>)m_pDataFile.m_ppDataObjects[Index]).Add(obj);
                        }
                    }
                }
                else
                {
                    CSystem.dbg_msg("datafile", "get data wrong version");
                    /*
                    // load the data
                    CSystem.dbg_msg("datafile", "loading data index={0} size={1}", Index, DataSize);
                    m_pDataFile.m_ppDataPtrs[Index] = Marshal.AllocHGlobal(DataSize);

                    int t = Marshal.ReadInt32(m_pDataFile.m_Info.m_pDataOffsets + sizeof(int) * Index);
                    CSystem.io_seek(m_pDataFile.m_File, m_pDataFile.m_DataStartOffset + t, SeekOrigin.Begin);
                    CSystem.io_read(m_pDataFile.m_File, m_pDataFile.m_ppDataPtrs[Index], DataSize);
                    */
                }
            }
            
            return (List<T>)m_pDataFile.m_ppDataObjects[Index];
        }

        public List<T> GetData<T>(int Index) where T : class, new()
        {
            return GetDataImpl<T>(Index);
        }

        public int GetItemSize(int Index)
        {
            if (m_pDataFile == null)
                return 0;

            if (Index == m_pDataFile.m_Header.m_NumItems - 1)
                return m_pDataFile.m_Header.m_ItemSize - m_pDataFile.m_Info.m_pItemOffsets[Index];
            return m_pDataFile.m_Info.m_pItemOffsets[Index + 1] - m_pDataFile.m_Info.m_pItemOffsets[Index];
        }

        public T GetItem<T>(int Index, ref int pType, ref int pID) where T : class, new()
        {
            if (m_pDataFile == null)
            {
                pType = 0;
                pID = 0;
                return null;
            }

            MemoryStream stream = new MemoryStream(m_pDataFile.m_pData);
            stream.Seek(m_pDataFile.m_Info.m_pItemStart - 36 + m_pDataFile.m_Info.m_pItemOffsets[Index], SeekOrigin.Begin);

            CDatafileItem i;
            CSystem.read_obj_from_stream(out i, stream);

            pType = (i.m_TypeAndID >> 16) & 0xffff; // remove sign extention
            pID = i.m_TypeAndID & 0xffff;

            T output;
            CSystem.read_obj_from_stream<T>(out output, stream);
            return output;
        }


        public void UnloadData(int Index)
        {
            if (Index < 0)
                return;
            /*
            //
            Marshal.FreeHGlobal(m_pDataFile.m_ppDataPtrs[Index]);
            m_pDataFile.m_ppDataPtrs[Index] = IntPtr.Zero;*/
        }

        public T FindItem<T>(int Type, int ID) where T : class, new()
        {
            if (m_pDataFile == null)
                return null;

            int Start = 0, Num = 0;
            GetType(Type, ref Start, ref Num);

            for (int i = 0; i < Num; i++)
            {
                int ItemID = -1;
                int iType = 0;
                T pItem = GetItem<T>(Start + i, ref iType, ref ItemID);
                if (ID == ItemID)
                    return pItem;
            }
            return null;
        }


        public void GetType(int Type, ref int pStart, ref int pNum)
        {
            pStart = 0;
            pNum = 0;

            if (m_pDataFile == null)
                return;

            for (int i = 0; i < m_pDataFile.m_Header.m_NumItemTypes; i++)
            {
                if (m_pDataFile.m_Info.m_pItemTypes[i].m_Type == Type)
                {
                    pStart = m_pDataFile.m_Info.m_pItemTypes[i].m_Start;
                    pNum = m_pDataFile.m_Info.m_pItemTypes[i].m_Num;
                }
            }
        }

        public int NumItems()
        {
            if (m_pDataFile == null)
                return 0;
            return m_pDataFile.m_Header.m_NumItems;
        }

        public bool Close()
        {
            if (m_pDataFile == null)
                return true;

            CSystem.io_close(m_pDataFile.m_File);
            m_pDataFile = null;
            return true;
        }

        public uint Crc()
        {
            if (m_pDataFile == null)
                return 0xFFFFFFFF;
            return m_pDataFile.m_Crc;
        }

        public bool IsOpen()
        {
            return m_pDataFile != null;
        }
    }
}
