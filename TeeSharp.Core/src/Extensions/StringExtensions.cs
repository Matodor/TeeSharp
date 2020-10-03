using System;

namespace TeeSharp.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToString(this ReadOnlySpan<char> array)
        {
            return array.ToString();
        }
    }
}