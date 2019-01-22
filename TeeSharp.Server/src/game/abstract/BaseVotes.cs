using System.Collections.Generic;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public class VoteOption
    {
        public const int MaxDescription = 64;
        public const int MaxCommand = 512;

        public string Description { get; set; }
        public string Command { get; set; }
    }

    public class ActiveVote
    {
        public int CallerId { get; set; }
        public string Description { get; set; }
        public string Reason { get; set; }
        public string Command { get; set; }
        public Vote Type { get; set; }
        public int CloseTick { get; set; }
        public int? ClientId { get; set; }

        public int VotesTotal { get; set; }
        public int VotesYes { get; set; }
        public int VotesNo { get; set; }
    }


    public abstract class BaseVotes : BaseInterface
    {
        protected struct PlayerVoteInfo
        {
            public int LastVoteTry { get; set; }
            public int LastVoteCall { get; set; }
            public int Vote { get; set; }
        }

        protected virtual ActiveVote ActiveVote { get; set; }
        protected virtual PlayerVoteInfo[] PlayersVoteInfo { get; set; }

        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }
        protected virtual BaseGameConsole Console { get; set; }
        
        protected virtual IDictionary<string, VoteOption> VoteOptions { get; set; }

        public abstract void Init();
        public abstract void Tick();

        public abstract bool AddVote(string description, string command);
        public abstract bool ContainsVote(string description);

        public abstract void ClientVote(GameMsg_ClVote message, BasePlayer player);
        public abstract void CallVote(GameMsg_ClCallVote message, BasePlayer player);
        public abstract void SendVotes(BasePlayer player);
        public abstract void SendVoteSet(Vote type, BasePlayer player);
        public abstract void SendVoteStatus(BasePlayer player);
        public abstract void ClearOptions(BasePlayer player);

        public abstract void StartVote(ActiveVote vote);
        public abstract void EndVote(Vote type);

        protected abstract void OnPlayerEnter(BasePlayer player);
        protected abstract void OnPlayerLeave(BasePlayer player, string reason);
        protected abstract void CheckVoteStatus();
    }
}