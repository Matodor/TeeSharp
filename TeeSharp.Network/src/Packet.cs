using System;
using System.Net;

namespace TeeSharp.Network
{
    public ref struct Packet
    {
        public IPEndPoint EndPoint { get; set; }
        public PacketFlags Flags { get; set; }
        public int ClientId { get; set; }
        public Span<byte> Data { get; set; }
    }
}