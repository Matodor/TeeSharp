using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

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
        bool isSixup,
        string? extraMsg = null)
    {
        SendConnectionStateMsg(
            client,
            endPoint,
            msg,
            token,
            ack,
            isSixup,
            extraData: extraMsg == null
                ? Array.Empty<byte>()
                : Encoding.UTF8.GetBytes(extraMsg)
        );
    }

    public static void SendConnectionStateMsg(
        UdpClient client,
        IPEndPoint endPoint,
        ConnectionStateMsg msg,
        SecurityToken? token,
        int ack,
        bool isSixup,
        byte[] extraData)
    {
        var data = new byte[1 + extraData.Length];

        var packet = new NetworkPacket(
            flags: PacketFlags.ConnectionState,
            ack: ack,
            chunksCount: 0,
            isSixup: isSixup,
            securityToken: token,
            responseToken: null,
            data: data,
            extraData: Array.Empty<byte>()
        ) { Data = { [0] = (byte) msg } };

        if (extraData.Length > 0)
            extraData.CopyTo(data, 1);

        SendPacket(client, endPoint, packet);
    }

    public static void SendPacket(
        UdpClient client,
        IPEndPoint endPoint,
        NetworkPacket packet)
    {
        if (packet.Data.Length == 0)
            return;


        throw new NotImplementedException();

        // var buffer = new Span<byte>(new byte[NetworkConstants.MaxPacketSize]);
        // var headerSize = NetworkConstants.PacketHeaderSize;
        //
        // if (isSixup)
        // {
        //     headerSize += StructHelper<SecurityToken>.Size;
        //     securityToken.CopyTo(buffer.Slice(NetworkConstants.PacketHeaderSize));
        // }
        // else if (securityToken != SecurityToken.Unsupported)
        // {
        //     securityToken.CopyTo(buffer.Slice(NetworkConstants.PacketHeaderSize));
        //     // asdasd
        // }
    }
}
