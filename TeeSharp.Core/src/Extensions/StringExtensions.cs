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

        public static ReadOnlySpan<char> SkipToWhitespaces(this ReadOnlySpan<char> input)
        {
            int index;
            for (index = 0; index < input.Length; index++)
            {
                if (input[index] == ' ' ||
                    input[index] == '\t' ||
                    input[index] == '\n')
                {
                    break;
                }
            }

            return input[index..];
        }

        public static ReadOnlySpan<char> SkipWhitespaces(this ReadOnlySpan<char> input)
        {
            int index;
            for (index = 0; index < input.Length; index++)
            {
                if (input[index] != ' ' &&
                    input[index] != '\t' &&
                    input[index] != '\n' &&
                    input[index] != '\r')
                {
                    break;
                }
            }

            return input[index..];
        }
    }
}