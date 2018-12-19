using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Core;
using TeeSharp.Network.Enums;
using TeeSharp.Network.Extensions;

namespace TeeSharp.Network
{
    public class TokenCache : BaseTokenCache
    {
        public TokenCache()
        {
            ConnlessPacketList = new List<ConnlessPacketInfo>();
            TokenCache = new List<AddressInfo>();
        }

        public override void Init(UdpClient client, BaseTokenManager tokenManager)
        {
            Client = client;
            ConnlessPacketList.Clear();
            TokenCache.Clear();
            TokenManager = tokenManager;
        }

        public override void SendPacketConnless(IPEndPoint endPoint, byte[] data, 
            int dataSize, SendCallbackData callbackData)
        {
            var token = GetToken(endPoint);
            if (token != TokenHelper.TokenNone)
            {
                NetworkHelper.SendPacketConnless(Client, endPoint, token,
                    TokenManager.GenerateToken(endPoint), data, dataSize);
            }
            else
            {
                FetchToken(endPoint);

                var now = Time.Get();
                var info = new ConnlessPacketInfo(dataSize)
                {
                    EndPoint = endPoint,
                    Expiry = now + Time.Freq() * TokenHelper.TokenCachePacketExpiry,
                    LastTokenRequest = now,
                };

                if (callbackData != null)
                {
                    info.SendCallback = callbackData.Callback;
                    info.CallbackContext = callbackData.Context;

                    callbackData.TrackID = info.TrackID;
                }
                else
                {
                    info.SendCallback = null;
                    info.CallbackContext = null;
                }

                Buffer.BlockCopy(data, 0, info.Data, 0, dataSize);
                ConnlessPacketList.Add(info);
            }
        }

        public override void PurgeStoredPacket(int trackId)
        {
            for (var i = 0; i < ConnlessPacketList.Count; i++)
            {
                if (ConnlessPacketList[i].TrackID == trackId)
                {
                    ConnlessPacketList.RemoveAt(i);
                    return;
                }
            }
        }

        public override void FetchToken(IPEndPoint endPoint)
        {
            NetworkHelper.SendConnectionMsgWithToken(Client, endPoint, 
                TokenHelper.TokenNone, 0, ConnectionMessages.Token, 
                TokenManager.GenerateToken(endPoint), true);
        }

        public override void AddToken(IPEndPoint endPoint, uint token, 
            TokenFlags tokenFlags)
        {
            if (token == TokenHelper.TokenNone)
                return;

            for (var i = 0; i < ConnlessPacketList.Count; i++)
            {
                var info = ConnlessPacketList[i];
                if (info.EndPoint.Compare(endPoint, true))
                {
                    info.SendCallback?.Invoke(info.TrackID, info.CallbackContext);
                    NetworkHelper.SendPacketConnless(Client, 
                        info.EndPoint, token, TokenManager.GenerateToken(info.EndPoint), 
                        info.Data, info.DataSize);

                    ConnlessPacketList.RemoveAt(i--);
                }
            }

            if (!tokenFlags.HasFlag(TokenFlags.ResponseOnly))
            {
                TokenCache.Add(new AddressInfo()
                {
                    EndPoint = endPoint,
                    Token = token,
                    Expiry = Time.Get() + Time.Freq() * TokenHelper.TokenCacheAddressExpiry
                });
            }
        }

        public override uint GetToken(IPEndPoint endPoint)
        {
            for (var i = TokenCache.Count - 1; i >= 0; i--)
            {
                if (TokenCache[i].EndPoint.Compare(endPoint, true))
                    return TokenCache[i].Token;
            }

            return TokenHelper.TokenNone;
        }

        public override void Update()
        {
            var now = Time.Get();

            for (var i = 0; i < TokenCache.Count; i++)
            {
                if (TokenCache[i].Expiry <= now)
                    TokenCache.RemoveAt(i--);
            }

            for (var i = 0; i < ConnlessPacketList.Count; i++)
            {
                if (ConnlessPacketList[i].LastTokenRequest + 2 * Time.Freq() <= now)
                {
                    FetchToken(ConnlessPacketList[i].EndPoint);
                    ConnlessPacketList[i].LastTokenRequest = now;
                }
            }

            for (var i = 0; i < ConnlessPacketList.Count; i++)
            {
                if (ConnlessPacketList[i].Expiry <= now) 
                    ConnlessPacketList.RemoveAt(i--);
            }
        }
    }
}