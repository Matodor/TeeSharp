using System;
using System.Threading;
using System.Threading.Tasks;

namespace TeeSharp.Server;

public interface IGameServer : IDisposable
{
    int Tick { get; }
    TimeSpan GameTime { get; }
    ServerState ServerState { get; }
    ServerSettings Settings { get; }

    Task RunAsync(CancellationToken cancellationToken);
    Task StopAsync();
}
