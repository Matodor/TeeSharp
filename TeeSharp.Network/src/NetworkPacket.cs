namespace TeeSharp.Network;

public class NetworkPacket
{
    /// <summary>
    ///
    /// </summary>
    public PacketFlags Flags { get; }

    /// <summary>
    ///
    /// </summary>
    public int Ack { get; }

    /// <summary>
    ///
    /// </summary>
    public int ChunksCount { get; }

    /// <summary>
    ///
    /// </summary>
    public SecurityToken? SecurityToken { get; }

    /// <summary>
    ///
    /// </summary>
    public SecurityToken? ResponseToken { get; }

    /// <summary>
    ///
    /// </summary>
    public bool IsSixup { get; }

    /// <summary>
    ///
    /// </summary>
    public byte[] Data { get; }

    /// <summary>
    /// Used only for master server info extended
    /// </summary>
    public byte[] ExtraData { get; }

    public NetworkPacket(
        PacketFlags flags,
        int ack,
        int chunksCount,
        bool isSixup,
        SecurityToken? securityToken,
        SecurityToken? responseToken,
        byte[] data,
        byte[] extraData)
    {
        Flags = flags;
        Ack = ack;
        ChunksCount = chunksCount;
        IsSixup = isSixup;
        SecurityToken = securityToken;
        ResponseToken = responseToken;
        Data = data;
        ExtraData = extraData;
    }
}
