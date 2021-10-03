using System;
using System.Runtime.InteropServices;

namespace TeeSharp.Core.Helpers
{
    public static class ClassHelper<T> where T : class
    {
        // ReSharper disable StaticMemberInGenericType
        public static readonly bool IsArray;
        public static readonly Type Type;
        // ReSharper restore StaticMemberInGenericType

        static ClassHelper()
        {
            Type = typeof(T);
            IsArray = Type.IsArray;
        }
    }
}
