using System;

namespace TeeSharp.Common.Protocol
{
    public abstract class BaseNetObject
    {
        public abstract int[] Serialize();
        public abstract void Deserialize(int[] data);
    }

    public abstract class BaseNetObject<T> : BaseNetObject where T : BaseNetObject
    {
        public abstract bool Compare(T other);
    }
}