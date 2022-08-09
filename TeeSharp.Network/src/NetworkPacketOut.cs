using System;

namespace TeeSharp.Network;

public ref struct NetworkPacketOut
{
    /// <summary>
    ///
    /// </summary>
    public NetworkPacketFlags Flags;

    /// <summary>
    ///
    /// </summary>
    public int Ack;

    /// <summary>
    ///
    /// </summary>
    public int NumberOfMessages;

    /// <summary>
    ///
    /// </summary>
    public readonly Span<byte> Data = new byte[NetworkConstants.MaxPayload];

    /// <summary>
    ///
    /// </summary>
    public int DataSize;

    public NetworkPacketOut(
        NetworkPacketFlags flags = NetworkPacketFlags.None,
        int ack = 0,
        int numberOfMessages = 0,
        int dataSize = 0)
    {
        Flags = flags;
        Ack = ack;
        NumberOfMessages = numberOfMessages;
        DataSize = dataSize;
    }
}
