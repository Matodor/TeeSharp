using Examples.BasicServer;
using Serilog;
using TeeSharp.Server;

const string consoleLogFormat =
    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}][{Level:u3}][{SourceContext}] {Message}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console(outputTemplate: consoleLogFormat)
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostOptions(options =>
    {
        options.ShutdownTimeout = TimeSpan.FromSeconds(10);
        options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
    })
    .ConfigureServices((context, services) =>
    {
        services.AddGameServer(context.Configuration);
        services.AddHostedService<ServerWorker>();
        services.AddSingleton<IGameServer, BasicGameServer>();
    })
    .UseSerilog()
    .Build();

await host.RunAsync();

