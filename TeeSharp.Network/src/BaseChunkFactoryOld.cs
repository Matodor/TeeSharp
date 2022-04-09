using System.Net;

namespace TeeSharp.Network;

public abstract class BaseChunkFactoryOld
{
    public NetworkPacket NetworkPacket { get; protected set; }

    protected bool HasError { get; set; }
    protected IPEndPoint EndPoint { get; set; }
    protected int ClientId { get; set; }
    protected BaseNetworkConnectionOld ConnectionOld { get; set; }
    protected int ProcessedChunks { get; set; }

    public abstract void Init();
    public abstract void Reset();
    public abstract void Start(IPEndPoint endPoint, BaseNetworkConnectionOld connectionOld, int clientId);
    public abstract bool TryGetMessage(out NetworkMessage netMsg);
}
