using System;

namespace TeeSharp.Network;

public ref struct NetworkPacketOut
{
    /// <summary>
    ///
    /// </summary>
    public PacketFlags Flags;

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
    public bool IsSixup;

    /// <summary>
    ///
    /// </summary>
    public readonly Span<byte> Data = new byte[NetworkConstants.MaxPayload];

    /// <summary>
    ///
    /// </summary>
    public int DataSize;

    public NetworkPacketOut(
        PacketFlags flags = PacketFlags.None,
        int ack = 0,
        int chunksCount = 0,
        bool isSixup = false,
        int dataSize = 0)
    {
        Flags = flags;
        Ack = ack;
        ChunksCount = chunksCount;
        IsSixup = isSixup;
        DataSize = dataSize;
    }
}
