using System;
using TeeSharp.Core.MinIoC;
using TeeSharp.Server;

namespace Examples.DefaultServer;

internal static class DefaultServer
{
    private static void Main(string[] args)
    {
        var services = new Container();
            
        // ReSharper disable RedundantTypeArgumentsOfMethod
        services.Register<BaseServer, TeeSharp.Server.DefaultServer>().AsSingleton();
        // ReSharper restore RedundantTypeArgumentsOfMethod

        var server = services.Resolve<BaseServer>();
        server.ConfigureServices(services);
        server.Init();
        server.Run();
    }
}