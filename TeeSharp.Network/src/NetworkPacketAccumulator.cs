namespace TeeSharp.Network;

public class NetworkPacketAccumulator
{
    public int NumberOfMessages;
    public int BufferSize;
    public readonly byte[] Buffer = new byte[NetworkConstants.MaxPayload];
}
