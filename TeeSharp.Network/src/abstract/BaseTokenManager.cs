using System;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Core;
using TeeSharp.Network.Extensions;

namespace TeeSharp.Network
{
    public abstract class BaseTokenManager : BaseInterface
    {
        protected virtual UdpClient Client { get; set; } 

        protected virtual uint GlobalToken { get; set; }
        protected virtual uint PreviousGlobalToken { get; set; }

        protected virtual long Seed { get; set; }
        protected virtual long NextSeedTime { get; set; }
        protected virtual long PreviousSeed { get; set; }
        protected virtual int SeedTime { get; set; }

        public abstract void Init(UdpClient client, int seedTime = NetworkHelper.SeedTime);
        public abstract void Update();
        public abstract void GenerateSeed();
        public abstract bool ProcessMessage(IPEndPoint endPoint, 
            ChunkConstruct chunkConstruct);
        public abstract bool CheckToken(IPEndPoint endPoint, uint token,
            uint responseToken, ref bool broadcastResponse);
        public abstract uint GenerateToken(IPEndPoint endPoint);
        public abstract uint GenerateToken(IPEndPoint endPoint, long seed);
    }
}