using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TeeSharp
{
    public static class Base
    {
        public static void DbgAssert(bool condition, string error)
        {
            if (!condition)
                DbgMessage("", error, ConsoleColor.Red);
        }

        public static void DbgMessage(string sys, string fmt, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(StrFormat(sys, fmt));
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static bool CompareArrays(byte[] target1, byte[] target2, int limit = 0)
        {
            var minLength = System.Math.Min(target1.Length, target2.Length);
            if (limit == 0 || minLength < limit)
            {
                if (target1.Length != target2.Length)
                    return false;

                for (var i = 0; i < target1.Length; i++)
                    if (target1[i] != target2[i])
                        return false;
                return true;
            }
            
            for (var i = 0; i < limit; i++)
                if (target1[i] != target2[i])
                    return false;
            return true;
        }

        // strings
        public static string StrFormat(string sys, string fmt)
        {
            return $"[{DateTime.Now:G}][{sys}] {fmt}";
        }

        /* makes sure that the string only contains the characters between 32 and 127 */
        public static string StrSanitizeStrong(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            var outStr = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if ((int) str[i] >= 32 && (int) str[i] <= 127)
                    outStr.Append(str[i]);
            }

            return outStr.ToString();
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
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetStoragePath(string applicationName)
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/" + applicationName;
        }
        
        // network
        public static bool CompareAddresses(IPEndPoint first, IPEndPoint second, bool comparePorts)
        {
            return first.Address.Equals(second.Address) && (!comparePorts || first.Port == second.Port);
        }
        
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

        public static byte[] ReceiveUdp(UdpClient client, ref IPEndPoint addr)
        {
            return client.Receive(ref addr);
        }

        public static int SendUdp(UdpClient client, IPEndPoint addr, byte[] data, int dataSize)
        {
            return client.Send(data, dataSize, addr);
        }
    }
}
