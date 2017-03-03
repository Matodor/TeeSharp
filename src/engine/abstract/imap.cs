using System;
using System.Collections.Generic;
using System.IO;

namespace Teecsharp
{
    public abstract class IMap : IInterface
    {
        public abstract List<T> GetData<T>(int Index) where T : class, new();
        public abstract void UnloadData(int Index);
        public abstract T GetItem<T>(int Index, ref int Type, ref int pID) where T : class, new();
        public abstract void GetType(int Type, ref int pStart, ref int pNum);
        public abstract T FindItem<T>(int Type, int ID) where T : class, new();
        public abstract int NumItems();
    }


    public abstract class IEngineMap : IMap
    {
        public abstract FileStream GetStream();
        public abstract bool Load(string pMapName);
        public abstract bool IsLoaded();
        public abstract void Unload();
        public abstract uint Crc();
    }
}
