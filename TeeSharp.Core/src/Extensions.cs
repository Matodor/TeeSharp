using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TeeSharp.Core
{
    public static class Extensions
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static bool ArrayCompare(this byte[] b1, byte[] compareArray, int limit = 0)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  

            if (limit == 0)
                return b1.Length == compareArray.Length && memcmp(b1, compareArray, b1.Length) == 0;
            return memcmp(b1, compareArray, limit) == 0;
        }

        public static T ReadStruct<T>(this FileStream fs)
        {
            var buffer = new byte[Marshal.SizeOf<T>()];
            fs.Read(buffer, 0, buffer.Length);

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var value = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();

            return value;
        }

        public static string Limit(this string source, int maxLength)
        {
            if (maxLength <= 0 || source.Length <= maxLength)
                return source;
            return source.Substring(0, maxLength);
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
                    input[i] != '\r' ||
                    input[i] != '\n' ||
                    input[i] != '\t')
                {
                    continue;
                }

                tmp.Append(input[i]);
            }

            return tmp.ToString();
        }
    }
}