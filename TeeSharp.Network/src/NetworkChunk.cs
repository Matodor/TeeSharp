using System.Net;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class NetworkChunk
    {
        public int ClientId;
        public SendFlags Flags;
        public IPEndPoint EndPoint;
        public int DataSize;
        public byte[] Data;
    }
}