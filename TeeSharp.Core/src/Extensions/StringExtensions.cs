using System;

namespace TeeSharp.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToString(this ReadOnlySpan<char> array)
        {
            return array.ToString();
        }
        
        public static string Limit(this string source, int maxLength)
        {
            return maxLength <= 0 || source.Length <= maxLength
                ? source
                : source.Substring(0, maxLength);
        }
    }
}