using System.Net;
using System.Net.Sockets;
using TeeSharp.Common;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class TokenManager : BaseTokenManager
    {
        public override void Init(UdpClient client, int seedTime = NetworkHelper.SeedTime)
        {
            Client = client;
            SeedTime = seedTime;

            GenerateSeed();
        }

        public override void Update()
        {
            if (Time.Get() > NextSeedTime)
                GenerateSeed();
        }

        public override void GenerateSeed()
        {
            PreviousSeed = Seed;

            for (var i = 0; i < 2; i++)
            {
                Seed = Seed << 32;
                Seed = Seed ^ RNG.Int();
            }

            PreviousGlobalToken = GlobalToken;
            GlobalToken = GenerateToken(null);
            NextSeedTime = Time.Get() + Time.Freq() * SeedTime;
        }

        public override bool ProcessMessage(IPEndPoint endPoint, ChunkConstruct packet)
        {
            var broadcastResponse = false;

            if (packet.Token != TokenHelper.TokenNone && 
                !CheckToken(endPoint, packet.Token, packet.ResponseToken, ref broadcastResponse))
            {
                return false;
            }

            var verified = packet.Token != TokenHelper.TokenNone;
            var tokenMessage = packet.Flags.HasFlag(PacketFlags.Control) &&
                               packet.Data[0] == (int) ConnectionMessages.Token;

            if (packet.Flags.HasFlag(PacketFlags.Connless) || !tokenMessage)
                return verified && !broadcastResponse;

            if (verified)
                return !broadcastResponse;

            if (packet.DataSize >= TokenHelper.TokenRequestDataSize)
            {
                NetworkHelper.SendConnectionMsgWithToken(Client, endPoint, packet.ResponseToken, 0,
                    ConnectionMessages.Token,
                    GenerateToken(endPoint), false);
            }

            return false;
        }

        public override bool CheckToken(IPEndPoint endPoint, uint token,
            uint responseToken, ref bool broadcastResponse)
        {
            var currentToken = GenerateToken(endPoint, Seed);
            if (currentToken == token)
                return true;

            if (GenerateToken(endPoint, PreviousSeed) == token)
                return true;

            if (token == GlobalToken || token == PreviousGlobalToken)
            {
                broadcastResponse = true;
                return true;
            }

            return false;
        }

        public override uint GenerateToken(IPEndPoint endPoint)
        {
            return TokenHelper.GenerateToken(endPoint, Seed);
        }

        public override uint GenerateToken(IPEndPoint endPoint, long seed)
        {
            return TokenHelper.GenerateToken(endPoint, seed);
        }
    }
}