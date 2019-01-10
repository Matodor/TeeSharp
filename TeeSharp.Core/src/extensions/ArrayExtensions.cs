using System;

namespace TeeSharp.Core.Extensions
{
    public static class ArrayExtensions
    {
        public static bool ArrayCompare(this byte[] b1, byte[] compareArray)
        {
            return b1.Equals(compareArray) || b1.AsSpan().SequenceEqual(compareArray);
        }

        public static bool ArrayCompare(this byte[] b1, byte[] compareArray, int limit)
        {
            return b1.Equals(compareArray) || b1.AsSpan(0, limit).SequenceEqual(compareArray);
        }
    }
}