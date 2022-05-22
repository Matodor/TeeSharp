namespace TeeSharp.Network;

public class NetworkPacketInSixup : NetworkPacketIn
{
    public SecurityToken SecurityToken { get; }

    public SecurityToken ResponseToken { get; }

    public NetworkPacketInSixup(
        PacketFlags flags,
        int ack,
        int chunksCount,
        byte[] data,
        byte[] extraData,
        SecurityToken securityToken,
        SecurityToken responseToken)
        : base(
            flags,
            ack,
            chunksCount,
            data,
            extraData
        )
    {
        SecurityToken = securityToken;
        ResponseToken = responseToken;
    }
}
