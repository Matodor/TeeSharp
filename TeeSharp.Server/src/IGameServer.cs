using System;
using System.Net;
using System.Threading;
using TeeSharp.Common.Protocol;
using TeeSharp.Network;

namespace TeeSharp.Server;

public interface IGameServer : IDisposable
{
    public delegate void MessageHandler(
        int connectionId,
        UnPacker unPacker,
        IPEndPoint endPoint,
        NetworkMessageFlags flags
    );

    int Tick { get; }
    int TickRate { get; }
    TimeSpan GameTime { get; }
    ServerState ServerState { get; }
    ServerSettings Settings { get; }

    void Run(CancellationToken cancellationToken);
    void Stop();
    void RegisterMessageHandler(ProtocolMessage msgId, MessageHandler handler);
}
