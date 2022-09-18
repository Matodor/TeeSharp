using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using TeeSharp.Core;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Network;

public static class NetworkHelper
{
    public static readonly HuffmanCompressor HuffmanCompressor;

    private static readonly uint[] Frequencies = new uint[]
    {
        1 << 30, 4545, 2657, 431, 1950, 919, 444, 482, 2244, 617, 838, 542, 715, 1814, 304, 240, 754, 212, 647, 186,
        283, 131, 146, 166, 543, 164, 167, 136, 179, 859, 363, 113, 157, 154, 204, 108, 137, 180, 202, 176,
        872, 404, 168, 134, 151, 111, 113, 109, 120, 126, 129, 100, 41, 20, 16, 22, 18, 18, 17, 19,
        16, 37, 13, 21, 362, 166, 99, 78, 95, 88, 81, 70, 83, 284, 91, 187, 77, 68, 52, 68,
        59, 66, 61, 638, 71, 157, 50, 46, 69, 43, 11, 24, 13, 19, 10, 12, 12, 20, 14, 9,
        20, 20, 10, 10, 15, 15, 12, 12, 7, 19, 15, 14, 13, 18, 35, 19, 17, 14, 8, 5,
        15, 17, 9, 15, 14, 18, 8, 10, 2173, 134, 157, 68, 188, 60, 170, 60, 194, 62, 175, 71,
        148, 67, 167, 78, 211, 67, 156, 69, 1674, 90, 174, 53, 147, 89, 181, 51, 174, 63, 163, 80,
        167, 94, 128, 122, 223, 153, 218, 77, 200, 110, 190, 73, 174, 69, 145, 66, 277, 143, 141, 60,
        136, 53, 180, 57, 142, 57, 158, 61, 166, 112, 152, 92, 26, 22, 21, 28, 20, 26, 30, 21,
        32, 27, 20, 17, 23, 21, 30, 22, 22, 21, 27, 25, 17, 27, 23, 18, 39, 26, 15, 21,
        12, 18, 18, 27, 20, 18, 15, 19, 11, 17, 33, 12, 18, 15, 19, 18, 16, 26, 17, 18,
        9, 10, 25, 22, 22, 17, 20, 16, 6, 16, 15, 20, 14, 18, 24, 335, 1517
    };

    static NetworkHelper()
    {
        HuffmanCompressor = new HuffmanCompressor(Frequencies);
    }

    public static bool TryGetLocalIpAddress([NotNullWhen(true)] out IPAddress? result)
    {
        UnicastIPAddressInformation? mostSuitableIp = null;
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var network in networkInterfaces)
        {
            if (network.OperationalStatus != OperationalStatus.Up)
                continue;

            var properties = network.GetIPProperties();

            if (properties.GatewayAddresses.Count == 0)
                continue;

            foreach (var address in properties.UnicastAddresses)
            {
                if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                    continue;

                if (IPAddress.IsLoopback(address.Address))
                    continue;

                if (!address.IsDnsEligible)
                {
                    mostSuitableIp ??= address;
                    continue;
                }

                // The best IP is the IP got from DHCP server
                if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                {
                    if (mostSuitableIp is not { IsDnsEligible: true })
                        mostSuitableIp = address;

                    continue;
                }

                result = address.Address;
                return true;
            }
        }

        result = mostSuitableIp?.Address;
        return mostSuitableIp != null;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static bool TryGetUdpClient(IPEndPoint? localEP, [NotNullWhen(true)] out UdpClient? client)
    {
        try
        {
            client = localEP == null
                ? new UdpClient()
                : new UdpClient(localEP);

            return true;
        }
        catch (Exception e)
        {
            client = null;
            return false;
        }
    }

    public static void SendData(
        UdpClient client,
        IPEndPoint endPoint,
        ReadOnlySpan<byte> data,
        ReadOnlySpan<byte> extraData = default)
    {
        var bufferSize = NetworkConstants.PacketConnectionLessDataOffset + data.Length + extraData.Length;
        if (bufferSize > NetworkConstants.MaxPacketSize)
            throw new Exception("Maximum packet size exceeded.");

        var buffer = new Span<byte>(new byte[bufferSize]);

        if (extraData.IsEmpty)
        {
            buffer
                .Slice(0, NetworkConstants.PacketConnectionLessDataOffset)
                .Fill(255);
        }
        else
        {
            NetworkConstants.PacketHeaderExtended.CopyTo(buffer);
            extraData
                .Slice(0, NetworkConstants.PacketExtraDataSize)
                .CopyTo(buffer.Slice(NetworkConstants.PacketHeaderExtended.Length));
        }

        data.CopyTo(buffer.Slice(NetworkConstants.PacketConnectionLessDataOffset));
        client.Send(buffer, endPoint);
    }

    public static void SendConnectionStateMsg(
        UdpClient client,
        IPEndPoint endPoint,
        ConnectionStateMsg msg,
        SecurityToken token,
        int ack,
        string? extraMsg = null)
    {
        SendConnectionStateMsg(
            client: client,
            endPoint: endPoint,
            msg: msg,
            token: token,
            ack: ack,
            extraData: extraMsg == null
                ? Array.Empty<byte>()
                : Encoding.UTF8.GetBytes(extraMsg)
        );
    }

    public static void SendConnectionStateMsg(
        UdpClient client,
        IPEndPoint endPoint,
        ConnectionStateMsg msg,
        SecurityToken token,
        int ack,
        byte[] extraData)
    {
        if (1 + extraData.Length > NetworkConstants.MaxPayload)
            return;

        var packet = new NetworkPacketOut(
            flags: NetworkPacketFlags.Connection,
            ack: ack,
            numberOfMessages: 0,
            dataSize: 1 + extraData.Length
        );

        packet.Data[0] = (byte) msg;

        if (extraData.Length > 0)
            extraData.CopyTo(packet.Data.Slice(1));

        SendPacket(
            client: client,
            endPoint: endPoint,
            token: token,
            packet: packet,
            useCompression: false
        );
    }

    public static void SendPacket(
        UdpClient client,
        IPEndPoint endPoint,
        SecurityToken token,
        NetworkPacketOut packet,
        bool useCompression)
    {
        if (packet.Data.Length == 0)
            return;

        var bufferSize = -1;
        var buffer = (Span<byte>) new byte[NetworkConstants.MaxPacketSize];
        var compressedSize = -1;

        if (token != SecurityToken.Unsupported)
        {
            if (packet.DataSize + StructHelper<SecurityToken>.Size > packet.Data.Length)
                return;

            token.CopyTo(packet.Data.Slice(packet.DataSize));
            packet.DataSize += StructHelper<SecurityToken>.Size;
        }

        if (useCompression)
        {
            compressedSize = HuffmanCompressor.Compress(
                packet.Data.Slice(packet.DataSize),
                buffer.Slice(NetworkConstants.PacketHeaderSize)
            );

            bufferSize = compressedSize;
            packet.Flags |= NetworkPacketFlags.Compression;
        }

        if (compressedSize <= 0 || compressedSize >= packet.DataSize)
        {
            bufferSize = packet.DataSize;
            packet.Data.CopyTo(buffer.Slice(NetworkConstants.PacketHeaderSize));
            packet.Flags &= ~NetworkPacketFlags.Compression;
        }

        if (bufferSize < 0)
            return;

        bufferSize += NetworkConstants.PacketHeaderSize;
        buffer[0] = (byte) ((((int) packet.Flags << 2) & 0b1111_1100) | ((packet.Ack >> 8) & 0b0000_0011));
        buffer[1] = (byte) (packet.Ack & 0b1111_1111);
        buffer[2] = (byte) (packet.NumberOfMessages & 0b1111_1111);

        try
        {
            client.Send(buffer.Slice(0, bufferSize), endPoint);
        }
        catch
        {
            // ignore
        }
    }

    public static bool IsSequenceInBackroom(int sequence, int ack)
    {
        var bottom = ack - NetworkConstants.MaxSequence / 2;
        if (bottom < 0)
        {
            if (sequence <= ack ||
                sequence >= (bottom + NetworkConstants.MaxSequence))
            {
                return true;
            }
        }
        else if (sequence <= ack && sequence >= bottom)
        {
            return true;
        }

        return false;
    }
}
