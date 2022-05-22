namespace TeeSharp.Network;

public class NetworkPacketIn
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
    public byte[] Data { get; }

    /// <summary>
    /// Used only for master server info extended
    /// </summary>
    public byte[] ExtraData { get; }

    public NetworkPacketIn(
        PacketFlags flags,
        int ack,
        int chunksCount,
        byte[] data,
        byte[] extraData)
    {
        Flags = flags;
        Ack = ack;
        ChunksCount = chunksCount;
        Data = data;
        ExtraData = extraData;
    }
}
