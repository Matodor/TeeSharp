using System.Runtime.InteropServices;

namespace TeeSharp.Core
{
    public static class TypeHelper<T>
    {
        public static readonly int Bytes = Marshal.SizeOf(typeof(T));
    }
}