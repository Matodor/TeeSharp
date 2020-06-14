using System;
using System.Net;
using System.Net.Sockets;

namespace TeeSharp.Network.Extensions
{
    public static class NetworkExtensions
    {
        private const int
            TypeInvalid = 0,
            TypeIPv4 = 1,
            TypeIPv6 = 2,
            LinkBroadcast = 4,
            TypeAll = TypeIPv4 | TypeIPv6;

        public static bool Compare(this IPEndPoint self, IPEndPoint other,
            bool comparePorts)
        {
            return self.Address.Equals(other.Address) && (!comparePorts || self.Port == other.Port);
        }

        /// <summary>
        /// Serialize IPEndPoint to 24 byte array [4 bytes endpoint type, 16 bytes ip address, 4 bytes port]
        /// </summary>
        /// <param name="endPoint">Serialized endpoint</param>
        /// <returns></returns>
        public static byte[] Raw(this IPEndPoint endPoint)
        {
            var array = new byte[sizeof(uint) + sizeof(byte) * 16 + sizeof(ushort) + 2]; // 0 type, 4 ip, 20 port, padding 2 bytes
            var bytes = new Span<byte>(array);
            var type = TypeInvalid;

            switch (endPoint.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    type = TypeIPv4;
                    break;
                case AddressFamily.InterNetworkV6:
                    type = TypeIPv6;
                    break;
            }
            
            BitConverter.TryWriteBytes(bytes, (uint) type);                         // type
            endPoint.Address.GetAddressBytes().AsSpan().CopyTo(bytes.Slice(4));     // address bytes
            BitConverter.TryWriteBytes(bytes.Slice(20), (ushort) endPoint.Port);    // port
            return array;
        }
    }
}