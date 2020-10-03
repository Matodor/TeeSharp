using System;
using System.IO;

namespace TeeSharp.Map
{
    public static class DataFileReader
    {
        public static bool Read(Stream stream, out string error)
        {
            var buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            
            // stream.Read
            
            error = string.Empty;

            return true;
        }
    }
}