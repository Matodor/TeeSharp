using System;
using System.Diagnostics.CodeAnalysis;
using TeeSharp.Network.Abstract;

namespace TeeSharp.Network.Concrete;

public class NetworkPacketUnpacker : INetworkPacketUnpacker
{
    public bool TryUnpack(Span<byte> buffer, [NotNullWhen(true)] out NetworkPacketIn? packet)
    {
        if (buffer.Length is < NetworkConstants.PacketHeaderSize or > NetworkConstants.MaxPacketSize)
        {
            packet = null;
            return false;
        }

        var flags = (NetworkPacketFlags) (buffer[0] >> 2);

        return flags.HasFlag(NetworkPacketFlags.ConnectionLess)
            ? TryUnpackConnectionLessPacket(buffer, out packet)
            : TryUnpackConnectionPacket(flags, buffer, out packet);
    }

    protected virtual bool TryUnpackConnectionPacket(
        NetworkPacketFlags flags,
        Span<byte> buffer,
        [NotNullWhen(true)] out NetworkPacketIn? packet)
    {
        var isSixup = flags.HasFlag(NetworkPacketFlags.Unused);
        if (isSixup ||
            buffer.Length < NetworkConstants.PacketHeaderSize)
        {
            packet = null;
            return false;
        }

        var ack = ((buffer[0] & 0b_0000_0011) << 8) | buffer[1];
        var numberOfMessages = buffer[2];
        byte[] data;

        if (flags.HasFlag(NetworkPacketFlags.Compression))
        {
            if (flags.HasFlag(NetworkPacketFlags.Connection))
            {
                packet = null;
                return false;
            }


            var decompressBuffer = new byte[NetworkConstants.MaxPayload].AsSpan();
            var decompressedSize = NetworkHelper.HuffmanCompressor.Decompress(
                buffer.Slice(NetworkConstants.PacketHeaderSize),
                decompressBuffer
            );

            if (decompressedSize < 0)
            {
                packet = null;
                return false;
            }

            data = decompressBuffer.Slice(0, decompressedSize).ToArray();
        }
        else
        {
            data = buffer.Slice(NetworkConstants.PacketHeaderSize).ToArray();
        }

        packet = new NetworkPacketIn(
            flags: flags,
            ack: ack,
            numberOfMessages: numberOfMessages,
            data: data,
            extraData: Array.Empty<byte>()
        );

        return true;
    }

    protected virtual bool TryUnpackConnectionLessPacket(
        Span<byte> buffer,
        [NotNullWhen(true)] out NetworkPacketIn? packet)
    {
        var isSixup = (buffer[0] & 0b_0000_0011) == 0b_0000_0001;
        if (isSixup ||
            buffer.Length < NetworkConstants.PacketConnectionLessDataOffset)
        {
            packet = null;
            return false;
        }

        var data = new byte[buffer.Length - NetworkConstants.PacketConnectionLessDataOffset];
        byte[] extraData;

        buffer
           .Slice(NetworkConstants.PacketConnectionLessDataOffset, data.Length)
           .CopyTo(data);

        if (buffer
            .Slice(0, NetworkConstants.PacketHeaderExtended.Length)
            .SequenceEqual(NetworkConstants.PacketHeaderExtended))
        {
            extraData = new byte[NetworkConstants.PacketExtraDataSize];

            buffer
               .Slice(NetworkConstants.PacketHeaderExtended.Length, extraData.Length)
               .CopyTo(extraData);
        }
        else
        {
            extraData = Array.Empty<byte>();
        }

        packet = new NetworkPacketIn(
            flags: NetworkPacketFlags.ConnectionLess,
            ack: 0,
            numberOfMessages: 0,
            data: data,
            extraData: extraData
        );

        return true;
    }
}
