using System;
using System.Diagnostics.CodeAnalysis;
using TeeSharp.Core.Extensions;
using TeeSharp.Network.Abstract;

namespace TeeSharp.Network.Concrete;

public class NetworkPacketUnpackerSixup : INetworkPacketUnpacker
{
    public bool TryUnpack(Span<byte> data,
        [NotNullWhen(true)] out NetworkPacket? packet,
        out bool isSixUp,
        out SecurityToken? securityToken,
        out SecurityToken? responseToken)
    {
        if (data.Length is < NetworkConstants.PacketHeaderSize or > NetworkConstants.MaxPacketSize)
        {
            packet = null;
            isSixUp = false;
            securityToken = null;
            responseToken = null;
            return false;
        }

        var packetFlags = (PacketFlags) (data[0] >> 2);

        int packetAck;
        int packetChunksCount;
        byte[] packetData;
        byte[] packetExtraData;

        if (packetFlags.HasFlag(PacketFlags.ConnectionLess))
        {
            isSixUp = (data[0] & 0b_0000_0011) == 0b_0000_0001;

            var dataStart = isSixUp
                ? NetworkConstants.PacketConnectionLessDataOffsetSixup
                : NetworkConstants.PacketConnectionLessDataOffset;

            if (dataStart > data.Length)
            {
                packet = null;
                securityToken = null;
                responseToken = null;
                return false;
            }

            if (isSixUp)
            {
                securityToken = data.Slice(1, 4).Deserialize<SecurityToken>();
                responseToken = data.Slice(5, 4).Deserialize<SecurityToken>();
            }

            packetFlags = PacketFlags.ConnectionLess;
            packetAck = 0;
            packetChunksCount = 0;
            packetData = new byte[data.Length - dataStart];

            data
                .Slice(dataStart, packetData.Length)
                .CopyTo(packetData);

            if (!isSixUp &&
                data.Slice(0, NetworkConstants.PacketHeaderExtended.Length)
                    .SequenceEqual(NetworkConstants.PacketHeaderExtended))
            {
                packetFlags |= PacketFlags.Extended;
                packetExtraData = new byte[NetworkConstants.PacketExtraDataSize];

                data.Slice(NetworkConstants.PacketHeaderExtended.Length, packetExtraData.Length)
                    .CopyTo(packetExtraData);
            }
            else
            {
                packetExtraData = Array.Empty<byte>();
            }
        }
        else
        {
            isSixUp = packetFlags.HasFlag(PacketFlags.Unused);

            var dataStart = isSixUp
                ? NetworkConstants.PacketHeaderSizeSixup
                : NetworkConstants.PacketHeaderSize;

            if (dataStart > data.Length)
            {
                packet = null;
                securityToken = null;
                responseToken = null;
                return false;
            }

            packetAck = ((data[0] & 0b_0000_0011) << 8) | data[1];
            packetChunksCount = data[2];
            packetData = new byte[data.Length - dataStart];
            packetExtraData = Array.Empty<byte>();

            if (isSixUp)
            {
                packetFlags = PacketFlags.None;

                // TODO: fix

                var sixUpFlags = (PacketFlagsSixUp) packetFlags;
                if (sixUpFlags.HasFlag(PacketFlagsSixUp.Connection))
                    packetFlags |= PacketFlags.ConnectionState;
                if (sixUpFlags.HasFlag(PacketFlagsSixUp.Resend))
                    packetFlags |= PacketFlags.Resend;
                if (sixUpFlags.HasFlag(PacketFlagsSixUp.Compression))
                    packetFlags |= PacketFlags.Compression;

                securityToken = data.Slice(3, 4).Deserialize<SecurityToken>();
            }

            if (packetFlags.HasFlag(PacketFlags.Compression))
            {
                if (packetFlags.HasFlag(PacketFlags.ConnectionState))
                {
                    packet = null;
                    securityToken = null;
                    responseToken = null;
                    return false;
                }

                throw new NotImplementedException();
            }
            else
            {
                data.Slice(dataStart, packetData.Length)
                    .CopyTo(packetData);
            }
        }

        packet = new NetworkPacket(
            packetFlags,
            packetAck,
            packetChunksCount,
            packetData,
            packetExtraData
        );

        securityToken = null;
        responseToken = null;
        return true;
    }
}
