using System;
using System.IO;

namespace TeeSharp.Core.Helpers;

/// <summary>
/// File system helper
/// </summary>
public static class FileHelper
{
    public static string WorkingPath(string? relativePath = null)
    {
        return FirstOrCombine(
            dir: AppDomain.CurrentDomain.BaseDirectory,
            relativePath: relativePath
        );
    }

    public static string AppDataPath(string? relativePath = null)
    {
        return FirstOrCombine(
            dir: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            relativePath: relativePath
        );
    }

    public static string FirstOrCombine(string dir, string? relativePath)
    {
        return string.IsNullOrEmpty(relativePath)
            ? dir
            : Path.Combine(dir, relativePath);
    }

    public static string FormatBytes(long bytes, int precision = 2)
    {
        if (bytes == 0) {
            return "0 B";
        }

        bytes = Math.Abs(bytes);

        var @base = Math.Log(bytes, 1024);
        var place = (int) Math.Floor(@base);
        var suffixes = new [] { "B", "KB", "MB", "GB", "TB" };

        return Math.Round(Math.Pow(1024, @base - place), precision) + " " + suffixes[place];
    }
}
