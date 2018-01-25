using System;

namespace TeeSharp.Common.NetObjects
{
    public abstract class BaseNetObj<T> where T : BaseNetObj<T>
    {
        public abstract bool Compare(T other);
        public abstract int[] Serialize();
        public abstract void Deserialize(int[] data);
    }
}