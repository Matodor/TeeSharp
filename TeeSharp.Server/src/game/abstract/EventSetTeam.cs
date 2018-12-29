using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
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

    public delegate void EventSetTeam(BasePlayer player, Team prevTeam, Team newTeam);
    public delegate void EventPlayerCharacter(BasePlayer player, Character character);

    public abstract class BasePlayer : BaseInterface
    {
        public event EventPlayerCharacter CharacterSpawned;
        public event EventSetTeam TeamChanged;

        public const Weapon WeaponGame = (Weapon) (-3);
        public const Weapon WeaponSelf = (Weapon) (-2);
        public const Weapon WeaponWorld = (Weapon) (-1);

        public int ClientId { get; private set; }
        public bool IsDummy { get; private set; }

        public virtual SpectatorMode SpectatorMode { get; set; }
        public virtual int SpectatorId { get; set; }
        public virtual int LastEmoteTick { get; set; }
        public virtual int LastChatTick { get; set; }
        public virtual int LastActionTick { get; set; }
        public virtual int LastSetTeamTick { get; set; }
        public virtual int LastKillTick { get; set; }
        public virtual int LastReadyChangeTick { get; set; }
        public virtual int LastSetSpectatorMode { get; set; }
        public virtual bool RespawnDisabled { get; set; }
        public virtual bool IsReadyToPlay { get; set; }
        public virtual int RespawnTick { get; set; }
        public virtual int DieTick { get; set; }

        public virtual int TeamChangeTick { get; protected set; }
        public virtual int[] ActualLatency { get; protected set; }
        public virtual Team Team { get; protected set; }
        public virtual Vector2 ViewPos { get; protected set; }
        public virtual Latency Latency { get; protected set; }
        public virtual TeeInfo TeeInfo { get; protected set; }
        public virtual bool IsReadyToEnter { get; protected set; }
        public virtual bool DeadSpectatorMode { get; protected set; }
        public virtual int LastChangeInfo { get; protected set; }

        protected virtual Character Character { get; set; }
        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }

        protected virtual PlayerFlags PlayerFlags { get; set; }
        protected virtual int InactivityTickCounter { get; set; }
        protected virtual Flag SpectatorFlag { get; set; }
        protected virtual bool ActiveSpectatorSwitch { get; set; }

        protected virtual bool Spawning { get; set; }
        protected virtual Activity LatestActivity { get; set; }


        public abstract void Tick();
        public abstract void PostTick();
        public abstract void Respawn();

        public abstract void OnSnapshot(int snappingClient, 
            out SnapshotPlayerInfo playerInfo,
            out SnapshotSpectatorInfo spectatorInfo, 
            out SnapshotDemoClientInfo demoClientInfo);

        public abstract void OnChangeInfo();
        public abstract void OnPlayerLeave(string reason);
        public abstract void OnPredictedInput(SnapshotPlayerInput input);
        public abstract void OnDirectInput(SnapshotPlayerInput input);
        public abstract Character GetCharacter();
        public abstract void KillCharacter(Weapon weapon);
        public abstract bool SetSpectatorID(SpectatorMode mode, int spectatorId);
        public abstract bool DeadCanFollow(BasePlayer player);

        public abstract void UpdateDeadSpecMode();
        public abstract void SetTeam(Team team);
        public abstract void ReadyToEnter();

        protected abstract void TryRespawn();

        public virtual void Init(int clientId, bool dummy)
        {
            ClientId = clientId;
            IsDummy = dummy;
        }

        protected void OnTeamChanged(Team prevTeam, Team newTeam)
        {
            TeamChanged?.Invoke(this, prevTeam, newTeam);
        }

        protected void OnCharacterSpawn(Character character)
        {
            CharacterSpawned?.Invoke(this, character);
        }
    }
}