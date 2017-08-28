using System;
using System.Diagnostics;
using TeeSharp.Server;
using TeeSharp;

namespace Examples.OverrideServer
{
    public class CustomServer : Server
    {
        protected override void NewClientCallback(int clientId)
        {
            Base.DbgMessage("server", "test message");

            base.NewClientCallback(clientId);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var server = Kernel.BindGet<IServer, CustomServer>(new CustomServer());
            server.Init(args);
            server.Run();
        }
    }
}