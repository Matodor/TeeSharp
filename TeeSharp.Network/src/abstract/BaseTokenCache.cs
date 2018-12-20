using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public abstract class BaseTokenCache : BaseInterface
    {
        protected struct AddressInfo
        {
            public IPEndPoint EndPoint;
            public uint Token;
            public long Expiry;
        }

        protected class ConnlessPacketInfo
        {
            public int TrackID { get; }
            public int DataSize { get; }
            public byte[] Data { get; }

            public IPEndPoint EndPoint { get; set; }
            public long Expiry { get; set; }
            public long LastTokenRequest { get; set; }
            public SendCallback SendCallback { get; set; }
            public object CallbackContext { get; set; }


            private static int _uniqueID = 0;

            public ConnlessPacketInfo(int dataSize)
            {
                DataSize = dataSize;
                Data = new byte[dataSize];
                TrackID = _uniqueID++;
            }
        }

        protected virtual BaseTokenManager TokenManager { get; set; }
        protected virtual UdpClient Client { get; set; }
        protected virtual IList<ConnlessPacketInfo> ConnlessPacketList { get; set; }
        protected virtual IList<AddressInfo> TokenCache { get; set; }

        public abstract void Init(UdpClient client, BaseTokenManager tokenManager);
        public abstract void SendPacketConnless(IPEndPoint endPoint,
            byte[] data, int dataSize, SendCallbackData callbackData = null);
        public abstract void PurgeStoredPacket(int trackId);
        public abstract void FetchToken(IPEndPoint endPoint);
        public abstract void AddToken(IPEndPoint endPoint, 
            uint token, TokenFlags tokenFlags);
        public abstract uint GetToken(IPEndPoint endPoint);
        public abstract void Update();
    }
}