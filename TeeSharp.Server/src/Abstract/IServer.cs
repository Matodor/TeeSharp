using System;
using System.Collections.Generic;
using System.Threading;

namespace TeeSharp.Server.Abstract;

public interface IServer : IDisposable
{
    int Tick { get; }
    int TickRate { get; }
    TimeSpan GameTime { get; }
    ServerState ServerState { get; }
    ServerSettings Settings { get; }
    IReadOnlyList<IServerClient> Clients { get; }

    void Run(CancellationToken cancellationToken);
    void Stop();
}
