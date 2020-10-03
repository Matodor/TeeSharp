using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TeeSharp.Network;

namespace TeeSharp.MasterServer
{
    public static class ServerEndpoint
    {
        /**
         * 16 bytes for IP address, 2 bytes for port
         */
        public const int SizeOfAddr = 18;
        
        public static IPEndPoint Get(Span<byte> data)
        {
            var isIpV4 = NetworkConstants.IpV4Mapping.AsSpan().SequenceEqual(data.Slice(0, 12));
            var port = (data[16] << 8) | data[17];

            return new IPEndPoint(new IPAddress(isIpV4 ? data.Slice(12, 4) : data), port);
        }
        
        public static IPEndPoint[] GetArray(Span<byte> data)
        {
            var array = new IPEndPoint[data.Length / SizeOfAddr];

            for (var i = 0; i < array.Length; i++)
                array[i] = Get(data.Slice(i * SizeOfAddr));

            return array;
        }
    }
}
