using System;
using System.Threading;

namespace TeeSharp.Server;

public interface IGameServer : IDisposable
{
    int Tick { get; }
    int TickRate { get; }
    TimeSpan GameTime { get; }
    ServerState ServerState { get; }
    ServerSettings Settings { get; }

    void Run(CancellationToken cancellationToken);
    void Stop();
}
