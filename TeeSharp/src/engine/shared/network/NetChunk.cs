using System.Net;

namespace TeeSharp
{
    public class NetChunk
    {
        public int ClientId;
        public SendFlag Flags;
        public IPEndPoint Address;
        public int DataSize;
        public byte[] Data;
    }
}
