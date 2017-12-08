using TeeSharp;
using TeeSharp.Server;

namespace Examples.BasicServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // register server singleton
            var server = Kernel.BindGet<IServer, Server>(new Server());
            server.Init(args);
            server.Run();
        }
    }
}