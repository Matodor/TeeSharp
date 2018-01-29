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

    public abstract class BasePlayer : BaseInterface
    {
        public virtual int ClientId { get; protected set; }

        public virtual ClientVersion ClientVersion { get; set; }
        public virtual PlayerFlags PlayerFlags { get; protected set; }
        public virtual Team Team { get; protected set; }

        public virtual bool IsReady { get; set; }
        public virtual long LastChangeInfo { get; set; }
        public virtual long LastChatMessage { get; set; }

        public virtual TeeInfo TeeInfo { get; protected set; }
        public virtual Latency Latency { get; protected set; }
        public virtual int SpectatorId { get; protected set; }
        public virtual vec2 ViewPos { get; protected set; }
        public virtual int[] ActLatency { get; protected set; }

        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }
        protected virtual Character Character { get; set; }

        public abstract Character GetCharacter();
        public abstract void Tick();
        public abstract void PostTick();
        public abstract void Respawn();

        public abstract void OnSnapshot(int snappingClient);
        public abstract void FakeSnapshot(int snappingClient);
        public abstract void OnPredictedInput(SnapObj_PlayerInput input);
        public abstract void OnDirectInput(SnapObj_PlayerInput input);

        public virtual void Init(int clientId, Team startTeam)
        {
            ClientId = clientId;
            
            Server = Kernel.Get<BaseServer>();
            GameContext = Kernel.Get<BaseGameContext>();
            Config = Kernel.Get<BaseConfig>();
        }
    }
}