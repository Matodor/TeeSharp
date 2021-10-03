using System;
using System.Runtime.InteropServices;

namespace TeeSharp.Core.Helpers
{
    public static class StructHelper<T> where T : struct
    {
        // ReSharper disable StaticMemberInGenericType
        public static readonly int Size;
        public static readonly int ElementSize;
        public static readonly bool IsArray;
        public static readonly Type Type;
        // ReSharper restore StaticMemberInGenericType

        static StructHelper()
        {
            Type = typeof(T);
            IsArray = Type.IsArray;
            Size = Marshal.SizeOf(Type);
            // ReSharper disable once AssignNullToNotNullAttribute
            ElementSize = Type.IsArray ? Marshal.SizeOf(Type.GetElementType()) : Size;
        }
    }
}
