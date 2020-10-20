using System;
using TeeSharp.Server;

namespace Examples.DefaultServer
{
    internal static class DefaultServer
    {
        private static void Main(string[] args)
        {
            var server = new TeeSharp.Server.DefaultServer();
            server.Init();
            server.Run();
        }
    }
}