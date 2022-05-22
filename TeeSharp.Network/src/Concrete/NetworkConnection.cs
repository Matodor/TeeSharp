using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Core.Helpers;
using TeeSharp.Network.Abstract;

namespace TeeSharp.Network.Concrete;

public class NetworkConnection : INetworkConnection
{
    public ConnectionState State { get; protected set; }

    /// <summary>
    /// Null when state is Offline
    /// </summary>
    public IPEndPoint EndPoint { get; protected set; }

    /// <summary>
    /// Null when state is Offline
    /// </summary>
    public bool IsSixup { get; protected set; }

    protected UdpClient Socket { get; set; }
    protected SecurityToken SecurityToken { get; set; }
    protected ILogger Logger { get; set; }

    public NetworkConnection(
        UdpClient socket,
        ILogger? logger = null)
    {
        Logger = logger ?? Tee.LoggerFactory.CreateLogger("NetworkConnection");
        Socket = socket;
        EndPoint = null!;
        IsSixup = false;
    }

    public virtual void Init(IPEndPoint endPoint, SecurityToken securityToken, bool isSixup)
    {
        Reset();

        State = ConnectionState.Online;
        EndPoint = endPoint;
        SecurityToken = securityToken;
        IsSixup = isSixup;
    }

    /// <summary>
    /// Process packet and return chunks
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    public IEnumerable<NetworkMessage> ProcessPacket(NetworkPacketIn packet)
    {
        var data = packet.Data.AsSpan();

        if (State != ConnectionState.Offline)
        {
            if (IsSixup)
            {
                // if (SecurityToken != зфс)
            }
            else
            {
                if (SecurityToken != SecurityToken.Unknown
                    && SecurityToken != SecurityToken.Unsupported)
                {
                    if (data.Length < StructHelper<SecurityToken>.Size)
                        yield break;

                    var token = (SecurityToken) data.Slice(data.Length - StructHelper<SecurityToken>.Size);

                    if (SecurityToken != token)
                    {
                        Logger.LogDebug(
                            "Token mismatch, expected {TokenExpected} got {TokenGot}",
                            SecurityToken,
                            token
                        );

                        yield break;
                    }
                }
            }
        }

        yield break;
    }

    protected virtual void Reset()
    {

    }
}
