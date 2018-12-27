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
        public abstract void SendClearMsg(int clientId);
        public abstract void SendVotes(int clientId);
        public abstract void SendActiveVote(int clientId);
        public abstract void PlayerConnected(int clientId);
        public abstract void PlayerDisconnected(int clientId);
        public abstract void PlayerChangeTeam(int clientId, Team prev, Team next);
    }
}