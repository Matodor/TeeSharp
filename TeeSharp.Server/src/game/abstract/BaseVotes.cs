using TeeSharp.Common.Config;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public class VoteOption
    {
        public string Description { get; set; }
        public string Command { get; set; }
    }

    public class ActiveVote
    {

    }

    public abstract class BaseVotes : BaseInterface
    {
        protected virtual ActiveVote ActiveVote { get; set; }
        protected virtual int[] LastVoteTry { get; set; }
        protected virtual int[] LastVoteCall { get; set; }

        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }
        
        public abstract void Init();
        public abstract void Tick();

        public abstract void CallVote(GameMsg_ClCallVote message, BasePlayer player);
        public abstract void SendVotes(BasePlayer player);

        protected abstract void OnPlayerEnter(BasePlayer player);
    }
}