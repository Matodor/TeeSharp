using System;
using System.Collections.Generic;
using System.IO;

namespace Teecsharp
{
    public class CMap : IEngineMap
    {
        private readonly CDataFileReader m_DataFile = new CDataFileReader();

        public override List<T> GetData<T>(int Index)
        {
            return m_DataFile.GetData<T>(Index);
        }

        public override void UnloadData(int Index)
        {
            m_DataFile.UnloadData(Index);
        }

        public override T GetItem<T>(int Index, ref int Type, ref int pID)
        {
            return m_DataFile.GetItem<T>(Index, ref Type, ref pID);
        }

        public override void GetType(int Type, ref int pStart, ref int pNum)
        {
            m_DataFile.GetType(Type, ref pStart, ref pNum);
        }

        public override T FindItem<T>(int Type, int ID)
        {
            return m_DataFile.FindItem<T>(Type, ID);
        }

        public override int NumItems()
        {
            return m_DataFile.NumItems();
        }

        public override FileStream GetStream()
        {
            return m_DataFile.GetStream();
        }

        public override bool Load(string pMapName)
        {
            IStorage pStorage = Kernel.RequestInterface<IStorage>();

            if (pStorage == null)
                return false;
            return m_DataFile.Open(pStorage, pMapName, IStorage.TYPE_ALL);
        }

        public override bool IsLoaded()
        {
            return m_DataFile.IsOpen();
        }

        public override void Unload()
        {
            m_DataFile.Close();
        }

        public override uint Crc()
        {
            return m_DataFile.Crc();
        }

        public static IEngineMap CreateEngineMap()
        {
            return new CMap();
        }
    }
}
