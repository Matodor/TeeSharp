using TeeSharp.Common.Enums;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseVotes : BaseInterface
    {
        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }

        public abstract void Init();
        public abstract void Tick();

        public abstract void SendClearMsg(BasePlayer player);
        public abstract void SendVotes(BasePlayer player);
        public abstract void SendActiveVote(BasePlayer player);

        protected abstract void OnPlayerReady(BasePlayer player);
    }
}