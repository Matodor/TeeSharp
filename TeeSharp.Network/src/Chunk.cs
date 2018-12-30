using System.Net;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class Chunk
    {
        public int ClientId { get; set; }
        public SendFlags Flags { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public int DataSize { get; set; }
        public byte[] Data { get; set; }
    }
}