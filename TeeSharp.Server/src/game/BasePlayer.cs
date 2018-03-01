using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
    public class TeeInfo
    {
        public string SkinName { get; set; }
        public bool UseCustomColor { get; set; }
        public int ColorBody { get; set; }
        public int ColorFeet { get; set; }
    }

    public class Latency
    {
        public int Accumulate;
        public int AccumulateMin;
        public int AccumulateMax;
        public int Average;
        public int Min;
        public int Max;
    }

    public class Activity
    {
        public int TargetX;
        public int TargetY;
    }

    public abstract class BasePlayer : BaseInterface
    {
        public virtual int ClientId { get; private set; }

        public virtual ClientVersion ClientVersion { get; set; }
        public virtual PlayerFlags PlayerFlags { get; protected set; } = 0;
        public virtual Team Team { get; protected set; }

        public string Name => Server.GetClientName(ClientId);
        public string Clan => Server.GetClientClan(ClientId);
        public int Country => Server.GetClientCountry(ClientId);
        
        public virtual bool IsReady { get; set; }
        public virtual int LastSetTeam { get; set; }
        public virtual int LastSetSpectatorMode { get; set; }
        public virtual int LastChangeInfo { get; set; }
        public virtual int LastChatMessage { get; set; }
        public virtual int LastActionTick { get; set; }
        public virtual int TeamChangeTick { get; set; }
        public virtual int RespawnTick { get; set; }
        public virtual int DieTick { get; set; }
        public virtual int SpectatorId { get; set; }

        public virtual TeeInfo TeeInfo { get; protected set; }
        public virtual Latency Latency { get; protected set; }
        public virtual Vec2 ViewPos { get; protected set; }
        public virtual int[] ActLatency { get; protected set; }

        protected virtual Activity LatestActivity { get; set; }
        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }
        protected virtual Character Character { get; set; }
        protected virtual bool Spawning { get; set; }

        public abstract Character GetCharacter();
        public abstract void Tick();
        public abstract void PostTick();
        public abstract void Respawn();
        public abstract void SetTeam(Team team);
        public abstract void KillCharacter(Weapon weapon = Weapon.GAME);

        public abstract void FakeSnapshot(int snappingClient);
        public abstract void OnDisconnect(string reason);
        public abstract void OnSnapshot(int snappingClient);
        public abstract void OnPredictedInput(SnapObj_PlayerInput input);
        public abstract void OnDirectInput(SnapObj_PlayerInput input);

        protected abstract void TryRespawn();

        public virtual void Init(int clientId, Team startTeam)
        {
            ClientId = clientId;
            
            Server = Kernel.Get<BaseServer>();
            GameContext = Kernel.Get<BaseGameContext>();
            Config = Kernel.Get<BaseConfig>();
        }
    }
}