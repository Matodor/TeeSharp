using System;

namespace TeeSharp.Network;

public class NetworkMessageHeader
{
    public NetworkMessageFlags Flags { get; private set; }
    public int Size { get; private set; }
    public int Sequence { get; private set; }

    public NetworkMessageHeader()
    {
        Flags = NetworkMessageFlags.None;
        Size = 0;
        Sequence = 0;
    }

    public Span<byte> Unpack(Span<byte> data)
    {
        Flags = (NetworkMessageFlags)(data[0] >> 6 & 3);
        Size = (data[0] & 0b_0011_1111) << 4 | data[1] & 0b0000_1111;
        Sequence = -1;

        if (!Flags.HasFlag(NetworkMessageFlags.Vital))
            return data.Slice(2);

        Sequence = (data[1] & -0b_0001_0000) << 2 | data[2];
        return data.Slice(3);
    }
}
