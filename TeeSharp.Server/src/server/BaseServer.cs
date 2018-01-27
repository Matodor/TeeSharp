using System.Net;
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
            MAX_SNAPSHOT_PACKSIZE = 900,
            SERVER_TICK_SPEED = 50;

        public abstract int MaxClients { get; }
        public abstract long Tick { get; protected set; }
        public abstract MapContainer CurrentMap { get; protected set; }

        protected abstract SnapshotBuilder SnapshotBuilder { get; set; }

        protected abstract BaseNetworkBan NetworkBan { get; set; }
        protected abstract BaseRegister Register { get; set; }
        protected abstract BaseGameContext GameContext { get; set; }
        protected abstract BaseConfig Config { get; set; }
        protected abstract BaseGameConsole Console { get; set; }
        protected abstract BaseStorage Storage { get; set; }
        protected abstract BaseNetworkServer NetworkServer { get; set; }

        protected abstract BaseServerClient[] Clients { get; set; }
        protected abstract long StartTime { get; set; }
        protected abstract bool IsRunning { get; set; }

        public abstract void SetClientName(int clientId, string name);
        public abstract void SetClientClan(int clientId, string clan);
        public abstract void SetClientCountry(int clientId, int country);

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

        protected abstract bool SendPackMsgBody<T>(T msg, MsgFlags flags, int clientId) where T : BaseGameMessage;
        protected abstract void StartNetworkServer();
        protected abstract void ProcessClientPacket(NetworkChunk packet);
        protected abstract void PumpNetwork();
        protected abstract void DoSnapshot();
        protected abstract long TickStartTime(long tick);
        protected abstract void DelClientCallback(int clientId, string reason);
        protected abstract void NewClientCallback(int clientid);

        protected abstract bool LoadMap(string mapName);
        protected abstract void SendMap(int clientId);

        protected abstract void RegisterConsoleCommands();
        protected abstract void SendRconLineAuthed(string message, object data);
        protected abstract void SendServerInfo(IPEndPoint endPoint, int token, bool showMore, int offset = 0);

        protected abstract void NetMsgPing(NetworkChunk packet, Unpacker unpacker, int clientId);
        protected abstract void NetMsgRconAuth(NetworkChunk packet, Unpacker unpacker, int clientId);
        protected abstract void NetMsgRconCmd(NetworkChunk packet, Unpacker unpacker, int clientId);
        protected abstract void NetMsgInput(NetworkChunk packet, Unpacker unpacker, int clientId);
        protected abstract void NetMsgEnterGame(NetworkChunk packet, Unpacker unpacker, int clientId);
        protected abstract void NetMsgReady(NetworkChunk packet, Unpacker unpacker, int clientId);
        protected abstract void NetMsgRequestMapData(NetworkChunk packet, Unpacker unpacker, int clientId);
        protected abstract void NetMsgInfo(NetworkChunk packet, Unpacker unpacker, int clientId);

        protected abstract void ConsoleReload(ConsoleResult result, object data);
        protected abstract void ConsoleLogout(ConsoleResult result, object data);
        protected abstract void ConsoleShutdown(ConsoleResult result, object data);
        protected abstract void ConsoleStatus(ConsoleResult result, object data);
        protected abstract void ConsoleKick(ConsoleResult result, object data);
    }
}