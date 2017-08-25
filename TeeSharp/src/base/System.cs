using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public static class System
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
