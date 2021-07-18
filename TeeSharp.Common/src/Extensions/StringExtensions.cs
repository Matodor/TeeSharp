using System;

namespace TeeSharp.Common.Extensions
{
    public static class StringExtensions
    {
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