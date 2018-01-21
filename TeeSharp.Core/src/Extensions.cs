using System.Runtime.InteropServices;

namespace TeeSharp.Core
{
    public static class Extensions
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static bool ArrayCompare(this byte[] b1, byte[] compareArray)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == compareArray.Length && memcmp(b1, compareArray, b1.Length) == 0;
        }
    }
}