using System.Net;

namespace TeeSharp.Network
{
    public class NetworkMessage
    {
        public int ClientId { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public MessageFlags Flags { get; set; }
        public int DataSize { get; set; }
        public byte[] Data { get; set; }
        public byte[] ExtraData { get; set; }
    }
}