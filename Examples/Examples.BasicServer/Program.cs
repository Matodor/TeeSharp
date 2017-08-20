using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeeSharp;
using TeeSharp.Server;

namespace Examples.BasicServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = Server.Create();
            server.Run();
            Console.ReadLine();
        }
    }
}
