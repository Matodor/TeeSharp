using System;
using System.Runtime.InteropServices;

namespace TeeSharp.Core.Helpers
{
    public static class TypeHelper<T>
    {
        // ReSharper disable StaticMemberInGenericType
        public static readonly int Size;
        public static readonly int ElementSize;
        public static readonly bool IsArray;
        public static readonly Type Type;
        // ReSharper restore StaticMemberInGenericType

        static TypeHelper()
        {
            Type = typeof(T);
            IsArray = Type.IsArray;
            Size = Marshal.SizeOf(Type);
            ElementSize = Type.IsArray ? Marshal.SizeOf(Type.GetElementType()) : Size;
        }
    }
}