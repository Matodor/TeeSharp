using System;

namespace TeeSharp.Network;

public class NetworkMessageHeader
{
    public bool IsVital => Flags.HasFlag(NetworkMessageHeaderFlags.Vital);

    public int Size { get; private set; }
    public int Sequence { get; private set; }

    protected NetworkMessageHeaderFlags Flags { get; set; }

    public NetworkMessageHeader()
    {
        Flags = NetworkMessageHeaderFlags.None;
        Size = 0;
        Sequence = 0;
    }

    public NetworkMessageHeader(NetworkMessageHeaderFlags flags, int size, int sequence)
    {
        Flags = flags;
        Size = size;
        Sequence = sequence;
    }

    public bool TryUnpack(Span<byte> data, out Span<byte> dataOut)
    {
        if (data.Length < 2)
        {
            dataOut = default;
            return false;
        }

        Flags = (NetworkMessageHeaderFlags)(data[0] >> 6 & 0b_0000_0011);
        Size = (data[0] & 0b_0011_1111) << 4 | data[1] & 0b_0000_1111;
        Sequence = -1;

        if (!Flags.HasFlag(NetworkMessageHeaderFlags.Vital))
        {
            dataOut = data.Slice(2);
            return true;
        }

        if (data.Length < 3)
        {
            dataOut = default;
            return false;
        }

        Sequence = (data[1] & -0b_0001_0000) << 2 | data[2];
        dataOut = data.Slice(3);
        return true;
    }

    public Span<byte> Pack(Span<byte> data)
    {
        data[0] = (byte)(((int) Flags & 3) << 6 | Size >> 4 & 0b_0011_1111);
        data[1] = (byte)(Size & 0b_0000_1111);

        if (!Flags.HasFlag(NetworkMessageHeaderFlags.Vital))
            return data.Slice(2);

        data[1] |= (byte) (Sequence >> 2 & -0b_0001_0000);
        data[2] = (byte)(Sequence & 0b_1111_1111);

        return data.Slice(3);
    }
}
