using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TeeSharp.Core.Helpers;
using TeeSharp.Server;

namespace TeeSharp.Tests
{
    public class ServerTests
    {
        [Test]
        public void ShouldServerIncreaseTicks()
        {
            const int error = 5;
            const int delay = 10000;
                        
            var server = new DefaultServer();
            server.Init();

            StartServer();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var beforeMilliseconds = stopWatch.ElapsedMilliseconds;
            
            Thread.Sleep(delay);
            server.Stop();

            var currentMilliseconds = (stopWatch.ElapsedMilliseconds - beforeMilliseconds);
            if (Math.Abs(currentMilliseconds / 1000 * 50 - server.Tick) < error)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
            
            
            async void StartServer()
            {
                await Task.Run(server.Run);
            }
        }
    }
}