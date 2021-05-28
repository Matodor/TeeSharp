using System;
using System.IO;

namespace TeeSharp.Core.Helpers
{
    /// <summary>
    /// File system helper
    /// </summary>
    public static class FileHelper
    {
        public static string WorkingPath(string relativePath = null)
        {
            return FirstOrCombine(
                dir: AppDomain.CurrentDomain.BaseDirectory,
                relativePath: relativePath
            );
        }

        public static string AppDataPath(string relativePath = null)
        {
            return FirstOrCombine(
                dir: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                relativePath: relativePath
            );
        }

        public static string FirstOrCombine(string dir, string relativePath)
        {
            return string.IsNullOrEmpty(relativePath)
                ? dir
                : Path.Combine(dir, relativePath);
        }
    }
}