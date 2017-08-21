using System;
using TeeSharp;
using TeeSharp.Server;

namespace Examples.BasicServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Kernel.Bind<IServer, Server>();

            var server = Kernel.Get<IServer>();
            server.Init(args);
            server.Run();

            Console.ReadLine();
        }
    }
}