using System;

namespace TeeSharp.Network;

public ref struct NetworkPacketOut
{
    /// <summary>
    ///
    /// </summary>
    public NetworkPacketInFlags Flags;

    /// <summary>
    ///
    /// </summary>
    public int Ack;

    /// <summary>
    ///
    /// </summary>
    public int ChunksCount;

    /// <summary>
    ///
    /// </summary>
    public readonly Span<byte> Data = new byte[NetworkConstants.MaxPayload];

    /// <summary>
    ///
    /// </summary>
    public int DataSize;

    public NetworkPacketOut(
        NetworkPacketInFlags flags = NetworkPacketInFlags.None,
        int ack = 0,
        int chunksCount = 0,
        bool isSixup = false,
        int dataSize = 0)
    {
        Flags = flags;
        Ack = ack;
        ChunksCount = chunksCount;
        DataSize = dataSize;
    }
}
