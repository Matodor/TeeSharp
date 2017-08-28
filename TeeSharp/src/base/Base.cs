using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace TeeSharp
{
    public static class Base
    {
        public static void DbgMessage(string sys, string fmt, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(StrFormat(sys, fmt));
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string StrFormat(string sys, string fmt)
        {
            return $"[{DateTime.Now:G}][{sys}] {fmt}";
        }

        public static long TimeFreq()
        {
            return Stopwatch.Frequency;
        }

        public static long TimeGet()
        {
            return Stopwatch.GetTimestamp();
        }

        // file system
        public static string GetCurrentWorkingDirectory()
        {
            return Environment.CurrentDirectory;
        }

        public static string GetStoragePath(string applicationName)
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/" + applicationName;
        }
        
        // network
        public static bool CreateUdpClient(IPEndPoint endPoint, out UdpClient client)
        {
            try
            {
                client = new UdpClient(endPoint) {Client = {Blocking = false}};
                return true;
            }
            catch
            {
                client = null;
                return false;
            }
        }
    }
}
