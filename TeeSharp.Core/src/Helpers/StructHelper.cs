using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TeeSharp.Core.Helpers;

public static class StructHelper<T> where T : struct
{
    // ReSharper disable StaticMemberInGenericType
    public static readonly int Size;
    public static readonly bool IsArray;
    public static readonly Type Type;
    // ReSharper restore StaticMemberInGenericType

    static StructHelper()
    {
        Type = typeof(T);
        IsArray = Type.IsArray;

        if (Unsafe.SizeOf<T>() != Marshal.SizeOf(Type))
            throw new Exception();

        if (Type == typeof(DateTime))
            Size = 8;
        else
            Size = Unsafe.SizeOf<T>();
    }
}
