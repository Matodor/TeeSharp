using Examples.BasicServer;
using Serilog;
using TeeSharp.Network;
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
        var serverSettings = context.Configuration.GetSection("TeeSharp:Server");

        services.Configure<ServerSettings>(serverSettings);
        services.AddHostedService<ServerWorker>();
    })
    .UseSerilog()
    .Build();

await host.RunAsync();

Log.CloseAndFlush();
