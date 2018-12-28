using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;
using TeeSharp.Network;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameContext : BaseInterface
    {
        public abstract string GameVersion { get; }
        public abstract string NetVersion { get; }
        public abstract string ReleaseVersion { get; }

        public virtual BaseVotes Votes { get; protected set; }
        public virtual BaseEvents Events { get; protected set; }
        public virtual BasePlayer[] Players { get; protected set; }
        public virtual BaseGameController GameController { get; protected set; }
        public virtual BaseGameWorld World { get; protected set; }

        public virtual BaseMapLayers MapLayers { get; set; }
        public virtual BaseMapCollision MapCollision { get; set; }

        protected virtual BaseTuningParams Tuning { get; set; }
        protected virtual BaseConfig Config { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseGameConsole Console { get; set; }
        protected virtual BaseGameMsgUnpacker GameMsgUnpacker { get; set; }

        protected virtual bool LockTeams { get; set; }

        public abstract void RegisterConsoleCommands();
        public abstract bool IsClientSpectator(int clientId);
        public abstract bool IsClientReady(int clientId);
        public abstract bool IsClientPlayer(int i);

        public abstract void CheckPureTuning();
        public abstract void SendTuningParams(int clientId);
        public abstract void SendMotd(int clientId);
        public abstract void SendSettings(int clientId);
        public abstract void SendChat(int from, ChatMode mode, int target, string message);
        public abstract void SendChatTarget(int clientId, string msg);
        public abstract void SendBroadcast(int clientId, string msg);
        public abstract void SendWeaponPickup(int clientId, Weapon weapon);
        public abstract void SendEmoticon(int clientId, Emoticon emote);
        public abstract void SendGameMessage(int clientId, GameplayMessage message, 
            int? param1 = null, int? param2 = null, int? param3 = null);

        public abstract void OnInit();
        public abstract void OnTick();
        public abstract void OnShutdown();
        public abstract void OnMessage(GameMessage msg, UnPacker unPacker, int clientId);
        public abstract void OnBeforeSnapshot();
        public abstract void OnAfterSnapshots();
        public abstract void OnSnapshot(int snappingId);
        public abstract void OnClientConnected(int clientId, bool dummy = false);
        public abstract void OnClientEnter(int clientId);
        public abstract void OnClientDisconnect(int clientId, string reason);
        public abstract void OnClientPredictedInput(int clientId, int[] input1);
        public abstract void OnClientDirectInput(int clientId, int[] input);

        protected abstract void OnMsgClientStartInfo(BasePlayer player, GameMsg_ClStartInfo startInfo);
        protected abstract void OnMsgClientSay(BasePlayer player, GameMsg_ClSay message);
        protected abstract void OnMsgClientSetTeam(BasePlayer player, GameMsg_ClSetTeam message);
        protected abstract void OnMsgClientEmoticon(BasePlayer player, GameMsg_ClEmoticon message);
        protected abstract void OnMsgClientKill(BasePlayer player, GameMsg_ClKill message);
        protected abstract void OnMsgClientReadyChange(BasePlayer player, GameMsg_ClReadyChange message);
        protected abstract void OnMsgClientSetSpectatorMode(BasePlayer player, GameMsg_ClSetSpectatorMode message);

        protected abstract GameMsg_SvClientInfo ClientInfo(int clientId);

        public abstract void CreateExplosion(Vector2 pos, int owner, Weapon weapon, bool noDamage);
        public abstract void CreatePlayerSpawn(Vector2 pos);
        public abstract void CreateDeath(Vector2 pos, int clientId);
        public abstract void CreateDamageInd(Vector2 pos, float a, int amount);
        public abstract void CreateHammerHit(Vector2 pos);
        public abstract void CreateSound(Vector2 pos, Sound sound, int mask = -1);
        public abstract void CreaetSoundGlobal(Sound sound, int targetId = -1);

        public static int MaskAll()
        {
            return -1;
        }

        public static int MaskOne(int clientID)
        {
            return 1 << clientID;
        }

        public static int MaskAllExceptOne(int clientId)
        {
            return 0b1111111_11111111_11111111_11111111 ^ MaskOne(clientId);
        }

        public static bool MaskIsSet(int mask, int clientID)
        {
            return (mask & MaskOne(clientID)) != 0;
        }
    }
}