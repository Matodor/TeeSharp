namespace TeeSharp.Network;

public class NetworkPacketIn
{
    /// <summary>
    ///
    /// </summary>
    public NetworkPacketInFlags Flags { get; }

    /// <summary>
    ///
    /// </summary>
    public int Ack { get; }

    /// <summary>
    ///
    /// </summary>
    public int NumberOfMessages { get; }

    /// <summary>
    ///
    /// </summary>
    public byte[] Data { get; }

    /// <summary>
    /// Used only for master server info extended
    /// </summary>
    public byte[] ExtraData { get; }

    public NetworkPacketIn(
        NetworkPacketInFlags flags,
        int ack,
        int numberOfMessages,
        byte[] data,
        byte[] extraData)
    {
        Flags = flags;
        Ack = ack;
        NumberOfMessages = numberOfMessages;
        Data = data;
        ExtraData = extraData;
    }
}
