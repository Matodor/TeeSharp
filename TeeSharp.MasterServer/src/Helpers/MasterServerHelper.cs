using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TeeSharp.Network;

namespace TeeSharp.MasterServer
{
    public static class MasterServerHelper
    {
        /**
         * 16 bytes for IP address, 2 bytes for port
         */
        public const int SizeOfAddr = 18;
        
        public static IPEndPoint EndPointDeserialize(Span<byte> data)
        {
            if (data.Length < SizeOfAddr)
                throw new ArgumentException(nameof(data));
            
            var isIpV4 = NetworkConstants.IpV4Mapping.AsSpan().SequenceEqual(data.Slice(0, 12));
            var port = (data[16] << 8) | data[17];

            return new IPEndPoint(new IPAddress(isIpV4 ? data.Slice(12, 4) : data), port);
        }
        
        public static IPEndPoint[] EndPointDeserializeMultiple(Span<byte> data)
        {
            if (data.Length < SizeOfAddr || data.Length % SizeOfAddr != 0)
                throw new ArgumentException(nameof(data));
            
            var array = new IPEndPoint[data.Length / SizeOfAddr];
            for (var i = 0; i < array.Length; i++)
                array[i] = EndPointDeserialize(data.Slice(i * SizeOfAddr));

            return array;
        }

        public static Span<byte> EndPointSerialize(IPEndPoint endPoint)
        {
            var buffer = new Span<byte>(new byte[SizeOfAddr]);
            NetworkConstants.IpV4Mapping.AsSpan().CopyTo(buffer);
            
            if (endPoint.Address.AddressFamily == AddressFamily.InterNetwork)
                endPoint.Address.TryWriteBytes(buffer.Slice(12, 4), out _);
            else
                endPoint.Address.TryWriteBytes(buffer, out _);
            
            buffer[16] = (byte) ((endPoint.Port >> 8) & 0b_11111111);
            buffer[17] = (byte) (endPoint.Port & 0b_11111111);

            return buffer;
        }
    }
}
