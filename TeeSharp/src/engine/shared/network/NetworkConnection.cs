using System.Net;
using System.Net.Sockets;

namespace TeeSharp
{
    public class NetworkConnection
    {
        public ConnectionState ConnectionState { get; private set; }
        public long ConnectionTime { get; private set; }
        public IPEndPoint PeerAddr { get; private set; }
        
        public void ResetStats()
        {
            
        }

        public void Reset()
        {
            ConnectionTime = 0;
            ConnectionState = ConnectionState.OFFLINE;
        }

        public void Init(UdpClient client, bool blockCloseMsg)
        {
            
        }

        public void AckChunks(int ack)
        {
            
        }

        public void SignalResend()
        {
            
        }

        public void Flush()
        {
            
        }

        public void QueueChunkEx(ChunkFlags flags, int dataSize, byte[] data, int sequence)
        {
            
        }

        public void QueueChunk(ChunkFlags flags, int dataSize, byte[] data)
        {
            
        }

        public void SendControl(ControlMessage message, int dataSize, byte[] data)
        {
            
        }

        public void ResendChunk()
        {
            
        }

        public void Resend()
        {
            
        }

        public bool Connect()
        {
            
        }

        public void Disconnect(string reason)
        {
            
        }

        public bool Feed(NetPacketConstruct packet, IPEndPoint addr)
        {
            
        }

        public void Update()
        {
            
        }

        public string ErrorString()
        {
            throw new System.NotImplementedException();
        }
    }
}