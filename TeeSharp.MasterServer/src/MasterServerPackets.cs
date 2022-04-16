using System;

namespace TeeSharp.MasterServer;

public static class MasterServerPackets
{
    public const int BufferOffset = 4;

    public static readonly byte[] Heartbeat        = Packet("bea2");
    public static readonly byte[] GetList          = Packet("req2");
    public static readonly byte[] List             = Packet("lis2");
    public static readonly byte[] GetCount         = Packet("cou2");
    public static readonly byte[] Count            = Packet("siz2");
    public static readonly byte[] GetInfo          = Packet("gie3");
    public static readonly byte[] Info             = Packet("inf3");
    public static readonly byte[] InfoExtended     = Packet("iext");
    public static readonly byte[] InfoExtendedMore = Packet("iex+");
    public static readonly byte[] FirewallCheck    = Packet("fw??");
    public static readonly byte[] FirewallResponse = Packet("fw!!");
    public static readonly byte[] FirewallOk       = Packet("fwok");
    public static readonly byte[] FirewallError    = Packet("fwer");

    private static byte[] Packet(string symbols)
    {
        var buffer = new Span<byte>(new byte[BufferOffset + symbols.Length]);
        buffer.Slice(0, BufferOffset).Fill(255);

        var bufferRest = buffer.Slice(BufferOffset);
        for (var i = 0; i < bufferRest.Length; i++)
            bufferRest[i] = (byte) symbols[i];

        return buffer.ToArray();
    }
}
