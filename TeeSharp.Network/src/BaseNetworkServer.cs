using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace TeeSharp.Network;

public abstract class BaseNetworkServer
{
    public EndPoint BindAddress => Socket?.Client.LocalEndPoint;
    public IReadOnlyList<BaseNetworkConnectionOld> Connections { get; protected set; }

    public NetworkServerConfig Config
    {
        get => _config;
        set
        {
            var oldConfig = _config;
            _config = value;
        }
    }

    protected byte[] SecurityTokenSeed { get; set; }

    private NetworkServerConfig _config;

    protected BaseChunkFactoryOld ChunkFactoryOld { get; set; }
    protected UdpClient Socket { get; set; }

    public abstract void Init(NetworkServerConfig config);
    public abstract void Update();
    // ReSharper disable once InconsistentNaming
    public abstract bool Open(IPEndPoint localEP);
    public abstract bool Receive(out NetworkMessage netMsg, ref SecurityToken responseToken);
    public abstract SecurityToken GetToken(IPEndPoint endPoint);
    public abstract int GetConnectionId(IPEndPoint endPoint);
    public abstract bool HasConnection(IPEndPoint endPoint);
    public abstract void SendConnStateMsg(IPEndPoint endPoint, ConnectionStateMsg connState,
        SecurityToken token, int ack = 0, bool isSixUp = false, string msg = null);
    public abstract void SendConnStateMsg(IPEndPoint endPoint, ConnectionStateMsg connState,
        SecurityToken token, int ack = 0, bool isSixUp = false, Span<byte> extraData = default);

    protected abstract void RefreshSecurityTokenSeed();
}
