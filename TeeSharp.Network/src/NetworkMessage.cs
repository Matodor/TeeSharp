using System.Net;

namespace TeeSharp.Network;

public class NetworkMessage
{
    /// <summary>
    ///
    /// </summary>
    public int ConnectionId { get; }

    /// <summary>
    ///
    /// </summary>
    public IPEndPoint EndPoint { get; }

    /// <summary>
    ///
    /// </summary>
    public NetworkMessageFlags Flags { get; }

    /// <summary>
    ///
    /// </summary>
    public SecurityToken ResponseToken { get; }

    /// <summary>
    ///
    /// </summary>
    public byte[] Data { get; }

    /// <summary>
    /// Used only for master server info extended
    /// </summary>
    public byte[] ExtraData { get; }

    public NetworkMessage(
        int connectionId,
        IPEndPoint endPoint,
        NetworkMessageFlags flags,
        SecurityToken responseToken,
        byte[] data,
        byte[] extraData)
    {
        ConnectionId = connectionId;
        EndPoint = endPoint;
        Flags = flags;
        ResponseToken = responseToken;
        Data = data;
        ExtraData = extraData;
    }
}
