using System.IO;

namespace TeeSharp.Core.Extensions
{
    public static class StreamExtensions
    {
        public static bool GetStruct<T>(this Stream stream, out T output) where T : struct
        {
            output = new T();
            return true;
        }
    }
}