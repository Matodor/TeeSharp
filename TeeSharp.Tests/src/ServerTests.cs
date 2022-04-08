using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TeeSharp.Server;

namespace TeeSharp.Tests;

public class ServerTests
{
    [Test]
    public void ShouldServerIncreaseTicks()
    {
        const int error = 15;
        const int delay = 10000;
                        
        var server = new DefaultServer();
        server.Init();
            
        var stopWatch = Stopwatch.StartNew();
        StartServer(server);
            
        Thread.Sleep(delay);
        server.Stop();
        stopWatch.Stop();
            
        var elapsedMilliseconds = stopWatch.ElapsedMilliseconds;
        var expectedTicks = elapsedMilliseconds / 1000 * BaseServer.TickRate;
            
        if (expectedTicks - server.Tick < error)
            Assert.Pass();
        else
            Assert.Fail();
    }

    private static async void StartServer(BaseServer server)
    {
        await Task.Run(server.Run);
    }
}
