using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Enums;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameController : BaseInterface
    {
        public abstract string GameType { get; }

        protected virtual GameFlags GameFlags { get; set; }
        protected virtual long RoundStartTick { get; set; }
        protected virtual long GameOverTick { get; set; }
        protected virtual int SuddenDeath { get; set; }
        protected virtual int Warmup { get; set; }
        protected virtual int RoundCount { get; set; }

        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }

        public abstract Team GetAutoTeam(int clientId);
        public abstract bool CheckTeamsBalance();

        public abstract void OnSnapshot(int snappingClient);
        public abstract void OnEntity(int tileIndex, Vector2 pos);
        public abstract void OnPlayerInfoChange(BasePlayer player);

        public virtual void Init()
        {
            Server = Kernel.Get<BaseServer>();
            GameContext = Kernel.Get<BaseGameContext>();
        }
    }
}