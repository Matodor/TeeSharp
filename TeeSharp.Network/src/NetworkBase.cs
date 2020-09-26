using System;
using System.Net;
using System.Net.Sockets;

namespace TeeSharp.Network
{
    public static class NetworkBase
    {
        public static void SendData(UdpClient client, IPEndPoint endPoint, ReadOnlySpan<byte> data)
        {
            const int dataOffset = 6;
            var bufferSize = dataOffset + data.Length;
            if (bufferSize > NetworkConstants.MaxPacketSize)
                throw new Exception("Maximum packet size exceeded.");
            
            var buffer = new Span<byte>(new byte[bufferSize]);
            buffer.Slice(0, dataOffset).Fill(255);
            data.CopyTo(buffer.Slice(dataOffset));
            
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
            client.EndSend(result);
        }
    }
}