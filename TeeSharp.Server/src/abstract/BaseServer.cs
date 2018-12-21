﻿using System.Net;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Common.Snapshots;
using TeeSharp.Common.Storage;
using TeeSharp.Core;
using TeeSharp.Map;
using TeeSharp.Network;
using TeeSharp.Server.Game;

namespace TeeSharp.Server
{
    public abstract class BaseServer : BaseInterface
    {
        public const int
            VANILLA_MAX_CLIENTS = 16,
            SERVER_TICK_SPEED = 50;

        public abstract int MaxClients { get; }
        public abstract int TickSpeed { get; }
        public virtual int Tick { get; protected set; }
        public virtual MapContainer CurrentMap { get; protected set; }
        public abstract int[] IdMap { get; protected set; }

        protected abstract SnapshotIdPool SnapshotIdPool { get; set; }

        protected virtual SnapshotBuilder SnapshotBuilder { get; set; }
        protected virtual BaseNetworkBan NetworkBan { get; set; }
        protected virtual BaseRegister Register { get; set; }
        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseConfig Config { get; set; }
        protected virtual BaseGameConsole Console { get; set; }
        protected virtual BaseStorage Storage { get; set; }
        protected virtual BaseNetworkServer NetworkServer { get; set; }

        protected virtual BaseServerClient[] Clients { get; set; }
        protected virtual long StartTime { get; set; }
        protected virtual bool IsRunning { get; set; }

        public abstract void SetClientName(int clientId, string name);
        public abstract void SetClientClan(int clientId, string clan);
        public abstract void SetClientCountry(int clientId, int country);

        public abstract bool GetClientInfo(int clientId, out ClientInfo info);
        public abstract string GetClientName(int clientId);
        public abstract string GetClientClan(int clientId);
        public abstract int GetClientCountry(int clientId);
        public abstract int GetClientScore(int clientId);
        public abstract bool ClientInGame(int clientId);

        public abstract void Init(string[] args);
        public abstract void Run();
        public abstract bool SendMsg(MsgPacker msg, MsgFlags flags, int clientId);
        public abstract bool SendMsgEx(MsgPacker msg, MsgFlags flags, int clientId, bool system);
        public abstract bool SendPackMsg<T>(T msg, MsgFlags flags, int clientId) where T : BaseGameMessage;

        public abstract T SnapObject<T>(int id) where T : BaseSnapObject, new();
        public abstract bool AddSnapItem<T>(T item, int id) where T : BaseSnapObject;

        protected abstract bool SendPackMsgBody<T>(T msg, MsgFlags flags, 
            int clientId) where T : BaseGameMessage;

        protected abstract bool SendPackMsgTranslate(GameMsg_SvEmoticon msg,
            MsgFlags flags, int clientId);

        protected abstract bool SendPackMsgTranslate(GameMsg_SvChat msg,
            MsgFlags flags, int clientId);

        protected abstract bool SendPackMsgTranslate(GameMsg_SvKillMsg msg,
            MsgFlags flags, int clientId);

        protected abstract bool SendPackMsgTranslate(BaseGameMessage msg,
            MsgFlags flags, int clientId);

        protected abstract bool SendPackMsgOne(BaseGameMessage msg,
            MsgFlags flags, int clientId);

        public abstract bool Translate(ref int targetId, int clientId);

        public abstract int SnapshotNewId();
        public abstract void SnapshotFreeId(int id);

        protected abstract bool StartNetworkServer();
        protected abstract void ProcessClientPacket(Chunk packet);
        protected abstract void PumpNetwork();
        protected abstract void DoSnapshot();
        protected abstract long TickStartTime(int tick);
        protected abstract void DelClientCallback(int clientId, string reason);
        protected abstract void NewClientCallback(int clientid, bool legacy);

        protected abstract bool LoadMap(string mapName);
        protected abstract void SendMap(int clientId);

        protected abstract void RegisterConsoleCommands();
        protected abstract void SendRconLine(int clientId, string line);
        protected abstract void SendRconLineAuthed(string message, object data);
        protected abstract void SendServerInfo(IPEndPoint endPoint, int token, bool showMore, int offset = 0);

        protected abstract void NetMsgPing(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgRconAuth(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgRconCmd(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgInput(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgEnterGame(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgReady(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgRequestMapData(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgInfo(Chunk packet, UnPacker unPacker, int clientId);

        protected abstract void ConsoleReload(ConsoleResult result, object data);
        protected abstract void ConsoleLogout(ConsoleResult result, object data);
        protected abstract void ConsoleShutdown(ConsoleResult result, object data);
        protected abstract void ConsoleStatus(ConsoleResult result, object data);
        protected abstract void ConsoleKick(ConsoleResult result, object data);

        public static int GetIdMap(int clientId)
        {
            return VANILLA_MAX_CLIENTS * clientId;
        }
    }
}