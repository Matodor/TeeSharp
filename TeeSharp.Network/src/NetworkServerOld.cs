// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Runtime.CompilerServices;
// using System.Security.Cryptography;
// using TeeSharp.Core.Helpers;
//
// namespace TeeSharp.Network;
//
// // ReSharper disable once ClassNeverInstantiated.Global
// public class NetworkServerOld : BaseNetworkServer
// {
//     public override bool Receive(out NetworkMessage netMsg, ref SecurityToken responseToken)
//     {
//         if (ChunkFactoryOld.TryGetMessage(out netMsg))
//             return true;
//     }
//
//     public override void SendConnStateMsg(IPEndPoint endPoint, ConnectionStateMsg connState,
//         SecurityToken token, int ack = 0, bool isSixUp = false, string msg = null)
//     {
//         NetworkHelper.SendConnectionStateMsg(Socket, endPoint, connState, token, ack, isSixUp, msg);
//     }
//
//     public override void SendConnStateMsg(IPEndPoint endPoint, ConnectionStateMsg connState,
//         SecurityToken token, int ack = 0, bool isSixUp = false, Span<byte> extraData = default)
//     {
//         NetworkHelper.SendConnectionStateMsg(Socket, endPoint, connState, token, ack, isSixUp, extraData);
//     }
//
//     protected virtual bool IsConnStateMsgWithToken(NetworkPacketOld packet)
//     {
//         if (ChunkFactoryOld.NetworkPacket.DataSize == 0 ||
//             !ChunkFactoryOld.NetworkPacket.Flags.HasFlag(PacketFlags.ConnectionState))
//         {
//             return false;
//         }
//
//         if (packet.Data[0] == (int) ConnectionStateMsg.Connect &&
//             packet.DataSize >= 1 + StructHelper<SecurityToken>.Size * 2 &&
//             packet.Data.AsSpan(1, StructHelper<SecurityToken>.Size) == SecurityToken.Magic)
//         {
//             return true;
//         }
//
//         if (packet.Data[0] == (int) ConnectionStateMsg.Accept &&
//             packet.DataSize >= 1 + StructHelper<SecurityToken>.Size)
//         {
//             return true;
//         }
//
//         return false;
//     }
//
//     /**
//          * Note: Dont use this method on existing connections for the specified `endPoint`
//          */
//     protected virtual void ProcessConnStateMsgWithToken(IPEndPoint endPoint, NetworkPacketOld packet)
//     {
//         var msg = (ConnectionStateMsg) packet.Data[0];
//         switch (msg)
//         {
//             case ConnectionStateMsg.Connect:
//                 var token = GetToken(endPoint);
//                 SendConnStateMsg(endPoint, ConnectionStateMsg.ConnectAccept,
//                     token, extraData: SecurityToken.Magic);
//                 break;
//
//             case ConnectionStateMsg.Accept:
//                 break;
//
//             default:
//                 // Log.Debug("[network] {Func}: Try process wrong msg type ({Code})",
//                 //     nameof(ProcessConnStateMsgWithToken), packet.Data[0]);
//                 break;
//         }
//     }
// }
