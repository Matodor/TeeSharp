using System.Runtime.InteropServices;

namespace TeeSharp.Core
{
    public static class TypeHelper<T>
    {
        public static readonly int Size = Marshal.SizeOf(typeof(T));
    }
}