using System;
using System.Net;
using TeeSharp.Network.Extensions;

namespace TeeSharp.Network
{
    public static class TokenHelper
    {
        public const int
            TokenCacheSize = 64,
            TokenCacheAddressExpiry = NetworkHelper.SeedTime,
            TokenCachePacketExpiry = 5,
            TokenRequestDataSize = 512;

        public const uint
            TokenMax = 0xffffffff,
            TokenNone = TokenMax,
            TokenMask = TokenMax;

        public static uint GenerateToken(IPEndPoint address, long seed)
        {
            var buffer = new byte[24 + sizeof(long)]; // sizeof(address raw) + sizeof(long)
            var bytes = new Span<byte>(buffer);
            
            // TODO
            //if (pAddr->type & NETTYPE_LINK_BROADCAST)
            //    return GenerateToken(&NullAddr, Seed);

            if (address != null)
            {
                address.Port = 0;
                address.Raw().CopyTo(bytes);
            }

            BitConverter.TryWriteBytes(bytes.Slice(24), seed);

            var result = NetworkHelper.Hash(buffer) & TokenMask;
            if (result == TokenNone)
                result--;

            return result;
        }
    }
}