using System.Text;

namespace TeeSharp.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToString(this char[] chars)
        {
            return new string(chars);
        }

        public static string Limit(this string source, int maxLength)
        {
            if (maxLength <= 0 || source.Length <= maxLength)
                return source;
            return source.Substring(0, maxLength);
        }

        public static string SkipWhitespaces(this string str)
        {
            return str.TrimStart(' ', '\t', '\n', '\r');
        }

        public static string SanitizeStrong(this string str)
        {
            var tmp = new StringBuilder(str);
            for (var i = 0; i < tmp.Length; i++)
            {
                tmp[i] = (char)(tmp[i] & 0x7f);
                if (tmp[i] < 32)
                    tmp[i] = (char)32;
            }
            return tmp.ToString();
        }

        public static string SanitizeCC(this string str)
        {
            var tmp = new StringBuilder(str.Length);
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] < 32)
                    continue;
                tmp.Append(str[i]);
            }

            return tmp.ToString();
        }

        public static string Sanitize(this string input)
        {
            var tmp = new StringBuilder(input.Length);
            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] < 32 ||
                    input[i] == '\r' ||
                    input[i] == '\n' ||
                    input[i] == '\t')
                {
                    continue;
                }

                tmp.Append(input[i]);
            }

            return tmp.ToString();
        }

        public static int[] StrToInts(this string input, int num)
        {
            byte[] bytes;
            var array = new int[num];

            if (!string.IsNullOrEmpty(input))
                bytes = Encoding.UTF8.GetBytes(input);
            else
                return array;

            var index = 0;
            for (var i = 0; i < array.Length; i++)
            {
                var buf = new[] { 0, 0, 0, 0 };
                for (var c = 0; c < buf.Length && index < bytes.Length; c++, index++)
                    buf[c] = (sbyte)bytes[index];

                array[i] = ((buf[0] + 128) << 24) |
                           ((buf[1] + 128) << 16) |
                           ((buf[2] + 128) << 08) |
                           ((buf[3] + 128) << 00);
            }

            array[array.Length - 1] = (int)(array[array.Length - 1] & 0xffff_ff00);
            return array;
        }

        /// <summary>
        /// Convert array of ints to ASCII string
        /// </summary>
        /// <param name="array">Input array</param>
        /// <returns></returns>
        public static string IntsToStr(this int[] array)
        {
            var bytes = new byte[array.Length * sizeof(int)];
            var count = 0;

            string GetString()
            {
                return Encoding.UTF8.GetString(bytes, 0, count);
            }

            for (var i = 0; i < array.Length; i++)
            {
                bytes[i * 4 + 0] = (byte)(((array[i] >> 24) & 0xFF) - 128);
                if (bytes[i * 4 + 0] < 32) return GetString();
                count++;

                bytes[i * 4 + 1] = (byte)(((array[i] >> 16) & 0xFF) - 128);
                if (bytes[i * 4 + 1] < 32) return GetString();
                count++;

                bytes[i * 4 + 2] = (byte)(((array[i] >> 8) & 0xFF) - 128);
                if (bytes[i * 4 + 2] < 32) return GetString();
                count++;

                bytes[i * 4 + 3] = (byte)((array[i] & 0xFF) - 128);
                if (bytes[i * 4 + 3] < 32) return GetString();
                count++;
            }

            count--;
            return GetString();
        }
    }
}