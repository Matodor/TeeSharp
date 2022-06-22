using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Network;

public static class NetworkHelper
{
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

    // public static void SendData(UdpClient client, IPEndPoint endPoint,
    //     ReadOnlySpan<byte> data,
    //     ReadOnlySpan<byte> extraData = default)
    // {
    //     var bufferSize = NetworkConstants.PacketConnectionLessDataOffset + data.Length;
    //     if (bufferSize > NetworkConstants.MaxPacketSize)
    //         throw new Exception("Maximum packet size exceeded.");
    //
    //     var buffer = new Span<byte>(new byte[bufferSize]);
    //     if (extraData.IsEmpty)
    //     {
    //         buffer
    //             .Slice(0, NetworkConstants.PacketConnectionLessDataOffset)
    //             .Fill(255);
    //     }
    //     else
    //     {
    //         NetworkConstants.PacketHeaderExtended.CopyTo(buffer);
    //         extraData
    //             .Slice(0, NetworkConstants.PacketExtraDataSize)
    //             .CopyTo(buffer.Slice(NetworkConstants.PacketHeaderExtended.Length));
    //     }
    //
    //     data.CopyTo(buffer.Slice(NetworkConstants.PacketConnectionLessDataOffset));
    //     client.BeginSend(
    //         buffer.ToArray(),
    //         buffer.Length,
    //         endPoint,
    //         EndSendCallback,
    //         client
    //     );
    // }
    //
    // private static void EndSendCallback(IAsyncResult result)
    // {
    //     var client = (UdpClient) result.AsyncState;
    //     client?.EndSend(result);
    // }

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
            flags: NetworkPacketInFlags.Connection,
            ack: ack,
            chunksCount: 0,
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
            token.CopyTo(packet.Data.Slice(packet.DataSize));
            packet.DataSize += StructHelper<SecurityToken>.Size;
        }

        if (useCompression)
        {
            compressedSize = 4; // TODO
            bufferSize = compressedSize;
            packet.Flags |= NetworkPacketInFlags.Compression;
        }

        if (compressedSize <= 0 || compressedSize >= packet.Data.Length)
        {
            bufferSize = packet.DataSize;
            packet.Data.CopyTo(buffer.Slice(NetworkConstants.PacketHeaderSize));
            packet.Flags &= ~NetworkPacketInFlags.Compression;
        }

        if (bufferSize < 0)
            return;

        bufferSize += NetworkConstants.PacketHeaderSize;
        buffer[0] = (byte) ((((int) packet.Flags << 2) & 0b1111_1100) | ((packet.Ack >> 8) & 0b0000_0011));
        buffer[1] = (byte) (packet.Ack & 0b1111_1111);
        buffer[2] = (byte) (packet.ChunksCount & 0b1111_1111);

        try
        {
            client.Send(buffer.Slice(0, bufferSize), endPoint);
        }
        catch
        {
            // ignore
        }
    }
}
