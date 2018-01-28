using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TeeSharp.Core
{
    public static class Extensions
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static int[] StrToInts(this string str, int num)
        {
            var ints = new int[num];

            if (string.IsNullOrEmpty(str))
                return ints;

            var index = 0;
            for (var i = 0; i < ints.Length; i++)
            {
                var buf = new[] { '\0', '\0', '\0', '\0' };
                for (var c = 0; c < buf.Length && index < str.Length; c++, index++)
                    buf[c] = str[index];
                
                ints[i] = ((buf[0] + 128) << 24) | 
                          ((buf[1] + 128) << 16) | 
                          ((buf[2] + 128) << 08) | 
                          ((buf[3] + 128) << 00);  
            }

            ints[ints.Length - 1] &= 0x7FFFFF00;
            return ints;
        }

        public static string IntsToStr(this int[] ints)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < ints.Length; i++)
            {
                do
                {
                    var c1 = (char) (((ints[i] >> 24) & 0b1111_1111) - 128);
                    if (c1 < 32) return builder.ToString();
                    builder.Append(c1);

                    var c2 = (char)(((ints[i] >> 16) & 0b1111_1111) - 128);
                    if (c2 < 32) return builder.ToString();
                    builder.Append(c2);

                    var c3 = (char)(((ints[i] >> 8) & 0b1111_1111) - 128);
                    if (c3 < 32) return builder.ToString();
                    builder.Append(c3);

                    var c4 = (char)((ints[i] & 0b1111_1111) - 128);
                    if (c4 < 32) return builder.ToString();
                    builder.Append(c4);

                } while (false);
            }
            return builder.ToString();
        }

        public static bool ArrayCompare(this byte[] b1, byte[] compareArray, int limit = 0)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  

            if (limit == 0)
                return b1.Length == compareArray.Length && memcmp(b1, compareArray, b1.Length) == 0;
            return memcmp(b1, compareArray, limit) == 0;
        }

        public static T[] ReadStructs<T>(this byte[] buffer)
        {
            var size = Marshal.SizeOf<T>();
            var array = new T[buffer.Length / size];
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var ptr = handle.AddrOfPinnedObject();

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = Marshal.PtrToStructure<T>(ptr + size * i);
            }

            handle.Free();
            return array;
        }

        public static T ReadStruct<T>(this byte[] buffer, int offset = 0)
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
            var value = Marshal.PtrToStructure<T>(ptr);

            handle.Free();
            return value;
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

        public static void CopyStream(this Stream input, Stream output)
        {
            var buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

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
    }
}