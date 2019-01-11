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
    public delegate void ClientEvent(int clientId);
    public delegate void ClientDisconnectEvent(int clientId, string reason);

    public abstract class BaseServer : BaseInterface
    {
        public event ClientEvent PlayerReady;
        public event ClientEvent PlayerEnter;
        public event ClientDisconnectEvent PlayerDisconnected;

        public const int MapChunkSize = NetworkHelper.MaxPayload - NetworkHelper.MaxChunkHeaderSize - 4;
        public const int ServerInfoFlagPassword = 1;

        public abstract int MaxClients { get; }
        public abstract int MaxPlayers { get; }
        public abstract int TickSpeed { get; }

        public virtual int Tick { get; protected set; }
        public virtual MapContainer CurrentMap { get; protected set; }

        protected virtual SnapshotIdPool SnapshotIdPool { get; set; }
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

        public abstract string ClientName(int clientId);
        public abstract void ClientName(int clientId, string name);

        public abstract string ClientClan(int clientId);
        public abstract void ClientClan(int clientId, string clan);

        public abstract int ClientCountry(int clientId);
        public abstract void ClientCountry(int clientId, int country);

        protected abstract void GenerateRconPassword();

        public abstract IPEndPoint ClientEndPoint(int clientId);
        public abstract ClientInfo ClientInfo(int clientId);

        public abstract bool ClientInGame(int clientId);

        public abstract void Init(string[] args);
        public abstract void Run();
        public abstract bool SendMsg(MsgPacker msg, MsgFlags flags, int clientId);
        public abstract bool SendPackMsg<T>(T msg, MsgFlags flags, int clientId)
            where T : BaseGameMessage;

        public abstract T SnapshotItem<T>(int id) where T : BaseSnapshotItem, new();
        public abstract bool SnapshotItem<T>(T item, int id) where T : BaseSnapshotItem;

        public abstract int SnapshotNewId();
        public abstract void SnapshotFreeId(int id);
        public abstract bool IsAuthed(int clientId);
        public abstract void Kick(int clientId, string reason);

        protected abstract bool StartNetworkServer();
        protected abstract void ProcessClientPacket(Chunk packet);
        protected abstract void PumpNetwork();
        protected abstract void DoSnapshot();
        protected abstract long TickStartTime(int tick);
        protected abstract void ClientDisconnected(int clientId, string reason);
        protected abstract void ClientConnected(int clientid);

        protected abstract bool LoadMap(string mapName);
        protected abstract void SendMap(int clientId);
        protected abstract void SendRconLine(int clientId, string line);
        protected abstract void SendRconLineAuthed(string message, object data);
        protected abstract void SendRconCommandAdd(ConsoleCommand command, int clientId);
        protected abstract void SendRconCommand(ConsoleCommand command, int clientId);

        protected abstract void RegisterConsoleCommands();
        protected abstract void GenerateServerInfo(Packer packer, int token);
        protected abstract void SendServerInfo(int clientId);

        protected abstract void NetMsgPing(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgRconAuth(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgRconCmd(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgInput(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgEnterGame(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgReady(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgRequestMapData(Chunk packet, UnPacker unPacker, int clientId);
        protected abstract void NetMsgInfo(Chunk packet, UnPacker unPacker, int clientId);

        protected void OnPlayerReady(int clientId)
        {
            PlayerReady?.Invoke(clientId);
        }

        protected void OnPlayerEnter(int clientId)
        {
            PlayerEnter?.Invoke(clientId);
        }

        protected void OnPlayerDisconnected(int clientId, string reason)
        {
            PlayerDisconnected?.Invoke(clientId, reason);
        }
    }
}