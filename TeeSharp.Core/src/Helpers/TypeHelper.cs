using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TeeSharp.Core
{
    public static class TypeHelper<T>
    {
        // ReSharper disable StaticMemberInGenericType
        public static readonly int Size;
        public static readonly int ElementSize;
        public static readonly bool IsArray;
        // ReSharper restore StaticMemberInGenericType

        static TypeHelper()
        {
            var type = typeof(T);
            IsArray = type.IsArray;
            Size = Marshal.SizeOf(type);
            ElementSize = type.IsArray ? Marshal.SizeOf(type.GetElementType()) : Size;
        }
    }
}