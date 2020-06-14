using System;

namespace TeeSharp.Common
{
    public static class FS
    {
        public static string WorkingDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}