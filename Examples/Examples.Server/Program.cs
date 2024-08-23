using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TeeSharp.Common;
using TeeSharp.Server;

namespace Examples.Server;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal class Program
{
    public static async Task Main(string[] args)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole()
        );

        var cts = new CancellationTokenSource();
        var config = new ServerConfig();
        var server = new TeeSharp.Server.Server(
            config: config,
            loggerFactory: loggerFactory
        );

        PosixSignalRegistration.Create(PosixSignal.SIGINT, context =>
        {
            cts.Cancel();
        });

        PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
        {
            cts.Cancel();
        });

        await server.RunAsync(cts.Token);
    }
}
