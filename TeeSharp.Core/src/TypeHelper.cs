using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TeeSharp.Core
{
    public static class TypeHelper<T>
    {
        public static readonly int Size = Unsafe.SizeOf<T>();
    }
}