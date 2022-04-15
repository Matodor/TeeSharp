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

    public static void SendPacket(UdpClient client, IPEndPoint endPoint,
        NetworkPacket packet, SecurityToken securityToken, bool isSixUp)
    {
        var buffer = new Span<byte>(new byte[NetworkConstants.MaxPacketSize]);
        var headerSize = NetworkConstants.PacketHeaderSize;

        if (isSixUp)
        {
            headerSize += StructHelper<SecurityToken>.Size;
            securityToken.CopyTo(buffer.Slice(NetworkConstants.PacketHeaderSize));
        }
        else if (securityToken != SecurityToken.Unsupported)
        {
            securityToken.CopyTo(buffer.Slice(NetworkConstants.PacketHeaderSize));
            // asdasd
        }
    }

    public static void SendData(UdpClient client, IPEndPoint endPoint,
        ReadOnlySpan<byte> data,
        ReadOnlySpan<byte> extraData = default)
    {
        var bufferSize = NetworkConstants.PacketConnectionLessDataOffset + data.Length;
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
        client.BeginSend(
            buffer.ToArray(),
            buffer.Length,
            endPoint,
            EndSendCallback,
            client
        );
    }

    private static void EndSendCallback(IAsyncResult result)
    {
        var client = (UdpClient) result.AsyncState;
        client?.EndSend(result);
    }

    public static void SendConnectionStateMsg(
        UdpClient client,
        IPEndPoint endPoint,
        ConnectionStateMsg state,
        SecurityToken token,
        int ack,
        bool isSixUp,
        string extraMsg)
    {
        if (string.IsNullOrEmpty(extraMsg))
        {
            SendConnectionStateMsg(client, endPoint, state, token, ack, isSixUp, Span<byte>.Empty);
        }
        else
        {
            var bufferLen = Encoding.UTF8.GetMaxByteCount(extraMsg.Length);
            var buffer = new Span<byte>(new byte[bufferLen]);
            var length = Encoding.UTF8.GetBytes(extraMsg.AsSpan(), buffer);
            SendConnectionStateMsg(client, endPoint, state, token, ack, isSixUp, buffer.Slice(0, length));
        }
    }

    public static void SendConnectionStateMsg(
        UdpClient client,
        IPEndPoint endPoint,
        ConnectionStateMsg state,
        SecurityToken token,
        int ack,
        bool isSixUp,
        Span<byte> extraData)
    {
        // var packet = new NetworkPacket(
        //     flags: PacketFlags.ConnectionState,
        //     ack: ack,
        //     chunksCount: 0,
        // );
        //
        // {
        //     ChunksCount = 0,
        //     DataSize = 1 + extraData.Length,
        // };
        //
        // packet.Data = new byte[packet.DataSize];
        // packet.Data[0] = (byte) state;
        //
        // if (!extraData.IsEmpty)
        //     extraData.CopyTo(packet.Data.AsSpan(1));
        //
        // // SendPacket(client, endPoint, packet);
    }
}
