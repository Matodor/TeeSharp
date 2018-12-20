using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TeeSharp.Core
{
    public static class Extensions
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static int[] StrToInts(this string input, int num)
        {
            var ints = new int[num];
            var bytes = new byte[0];
            var index = 0;

            if (!string.IsNullOrEmpty(input))
                bytes = Encoding.UTF8.GetBytes(input);

            for (var i = 0; i < ints.Length; i++)
            {
                var buf = new int[] { 0, 0, 0, 0 };
                for (int c = 0; c < buf.Length && index < bytes.Length; c++, index++)
                {
                    buf[c] = bytes[index] >= 128
                        ? bytes[index] - 256
                        : bytes[index];
                }

                ints[i] = ((buf[0] + 128) << 24) | 
                          ((buf[1] + 128) << 16) | 
                          ((buf[2] + 128) << 08) | 
                          ((buf[3] + 128) << 00);  
            }

            ints[ints.Length - 1] = (int) (ints[ints.Length - 1] & 0xffffff00);
            return ints;
        }

        public static string IntsToStr(this int[] ints)
        {
            var bytes = new byte[ints.Length * 4];
            var count = 0;

            string GetString()
            {
                return Encoding.UTF8.GetString(bytes, 0, count);
            }

            for (var i = 0; i < ints.Length; i++)
            {
                bytes[i * 4 + 0] = (byte) (((ints[i] >> 24) & 0b1111_1111) - 128);
                if (bytes[i * 4 + 0] < 32) return GetString();
                count++;

                bytes[i * 4 + 1] = (byte) (((ints[i] >> 16) & 0b1111_1111) - 128);
                if (bytes[i * 4 + 1] < 32) return GetString();
                count++;

                bytes[i * 4 + 2] = (byte) (((ints[i] >> 8) & 0b1111_1111) - 128);
                if (bytes[i * 4 + 2] < 32) return GetString();
                count++;

                bytes[i * 4 + 3] = (byte) ((ints[i] & 0b1111_1111) - 128);
                if (bytes[i * 4 + 3] < 32) return GetString();
                count++;
            }

            return GetString();
        }

        public static bool ArrayCompare(this byte[] b1, byte[] compareArray, int limit = 0)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  

            if (limit == 0)
                return b1.Length == compareArray.Length && memcmp(b1, compareArray, b1.Length) == 0;
            return memcmp(b1, compareArray, limit) == 0;
        }

        public static object ReadStructs(this byte[] buffer, Type type, int offset = 0)
        {
            var size = Marshal.SizeOf(type);
            var array = Array.CreateInstance(type, (buffer.Length - offset) / size);

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var ptr = handle.AddrOfPinnedObject();

            for (var i = 0; i < array.Length; i++)
                array.SetValue(Marshal.PtrToStructure(ptr + (size * i + offset), type), i);

            handle.Free();
            return array;
        }

        public static T[] ReadStructs<T>(this byte[] buffer, int offset = 0)
        {
            return (T[]) ReadStructs(buffer, typeof(T), offset);
        }

        public static object ReadStruct(this byte[] buffer, Type type, int offset = 0)
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var value = Marshal.PtrToStructure(handle.AddrOfPinnedObject() + offset, type);

            handle.Free();
            return value;
        }

        public static T ReadStruct<T>(this byte[] buffer, int offset = 0)
        {
            return (T) ReadStruct(buffer, typeof(T), offset);
        }

        public static object ReadStruct(this Stream fs, Type type)
        {
            var buffer = new byte[Marshal.SizeOf(type)];
            fs.Read(buffer, 0, buffer.Length);

            return ReadStruct(buffer, type);
        }

        public static T ReadStruct<T>(this Stream fs)
        {
            return (T) ReadStruct(fs, typeof(T));
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

        public static string SkipWhitespaces(this string str)
        {
            return str.TrimStart(' ', '\t', '\n', '\r');
        }

        public static string SanitizeStrong(this string str)
        {
            var tmp = new StringBuilder(str);
            for (var i = 0; i < tmp.Length; i++)
            {
                tmp[i] = (char) (tmp[i] & 0x7f);
                if (tmp[i] < 32)
                    tmp[i] = (char) 32;
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

        //public static uint ToUInt32(this byte[] array, int offset = 0)
        //{
        //    return (uint) (
        //       (array[0 + offset] << 24) | 
        //       (array[1 + offset] << 16) | 
        //       (array[2 + offset] << 8) |
        //       (array[3 + offset])
        //    );
        //}

        //public static void ToByteArray(this uint value, byte[] dstArray, 
        //    int offset)
        //{
        //    dstArray[0 + offset] = (byte) ((value & 0xff000000) >> 24);
        //    dstArray[1 + offset] = (byte) ((value & 0x00ff0000) >> 16);
        //    dstArray[2 + offset] = (byte) ((value & 0x0000ff00) >> 8);
        //    dstArray[3 + offset] = (byte) ((value & 0x000000ff) >> 0);
        //}
    }
}