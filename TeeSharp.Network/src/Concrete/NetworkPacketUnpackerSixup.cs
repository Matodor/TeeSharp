using System;
using System.Diagnostics.CodeAnalysis;
using TeeSharp.Core.Extensions;
using TeeSharp.Network.Abstract;

namespace TeeSharp.Network.Concrete;

public class NetworkPacketUnpackerSixup : INetworkPacketUnpacker
{

    public bool TryUnpack(Span<byte> buffer, [NotNullWhen(true)] out NetworkPacket? packet)
    {
        if (buffer.Length is < NetworkConstants.PacketHeaderSize or > NetworkConstants.MaxPacketSize)
        {
            packet = null;
            return false;
        }

        var flags = (PacketFlags) (buffer[0] >> 2);

        bool isSixup;
        int ack;
        int chunksCount;
        byte[] data;
        byte[] extraData;
        SecurityToken? securityToken = null;
        SecurityToken? responseToken = null;

        if (flags.HasFlag(PacketFlags.ConnectionLess))
        {
            isSixup = (buffer[0] & 0b_0000_0011) == 0b_0000_0001;

            var dataStart = isSixup
                ? NetworkConstants.PacketConnectionLessDataOffsetSixup
                : NetworkConstants.PacketConnectionLessDataOffset;

            if (dataStart > buffer.Length)
            {
                packet = null;
                return false;
            }

            if (isSixup)
            {
                securityToken = (SecurityToken) buffer.Slice(1, 4);
                responseToken = (SecurityToken) buffer.Slice(5, 4);
            }

            flags = PacketFlags.ConnectionLess;
            ack = 0;
            chunksCount = 0;
            data = new byte[buffer.Length - dataStart];

            buffer
                .Slice(dataStart, data.Length)
                .CopyTo(data);

            if (!isSixup &&
                buffer.Slice(0, NetworkConstants.PacketHeaderExtended.Length)
                    .SequenceEqual(NetworkConstants.PacketHeaderExtended))
            {
                flags |= PacketFlags.Extended;
                extraData = new byte[NetworkConstants.PacketExtraDataSize];

                buffer.Slice(NetworkConstants.PacketHeaderExtended.Length, extraData.Length)
                    .CopyTo(extraData);
            }
            else
            {
                extraData = Array.Empty<byte>();
            }
        }
        else
        {
            isSixup = flags.HasFlag(PacketFlags.Unused);

            var dataStart = isSixup
                ? NetworkConstants.PacketHeaderSizeSixup
                : NetworkConstants.PacketHeaderSize;

            if (dataStart > buffer.Length)
            {
                packet = null;
                return false;
            }

            ack = ((buffer[0] & 0b_0000_0011) << 8) | buffer[1];
            chunksCount = buffer[2];
            data = new byte[buffer.Length - dataStart];
            extraData = Array.Empty<byte>();

            if (isSixup)
            {
                flags = PacketFlags.None;

                // TODO: fix

                var sixUpFlags = (PacketFlagsSixUp) flags;
                if (sixUpFlags.HasFlag(PacketFlagsSixUp.Connection))
                    flags |= PacketFlags.ConnectionState;
                if (sixUpFlags.HasFlag(PacketFlagsSixUp.Resend))
                    flags |= PacketFlags.Resend;
                if (sixUpFlags.HasFlag(PacketFlagsSixUp.Compression))
                    flags |= PacketFlags.Compression;

                securityToken = buffer.Slice(3, 4).Deserialize<SecurityToken>();
            }

            if (flags.HasFlag(PacketFlags.Compression))
            {
                if (flags.HasFlag(PacketFlags.ConnectionState))
                {
                    packet = null;
                    return false;
                }

                throw new NotImplementedException();
            }
            else
            {
                buffer.Slice(dataStart, data.Length)
                    .CopyTo(data);
            }
        }

        packet = new NetworkPacket(
            flags,
            ack,
            chunksCount,
            isSixup,
            securityToken,
            responseToken,
            data,
            extraData
        );

        return true;
    }
}
