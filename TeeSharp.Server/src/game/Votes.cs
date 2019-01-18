using System;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game
{
    public class Votes : BaseVotes
    {
        public override void Init()
        {
            GameContext = Kernel.Get<BaseGameContext>();
            GameContext.PlayerEnter += OnPlayerEnter;

            Config = Kernel.Get<BaseConfig>();
            Server = Kernel.Get<BaseServer>();

            ActiveVote = null;
        }

        protected override void OnPlayerEnter(BasePlayer player)
        {
            Server.SendPackMsg(new GameMsg_SvVoteClearOptions(), MsgFlags.Vital, player.ClientId);
            SendVotes(player);
        }

        //protected override void OnPlayerReady(BasePlayer player)
        //{
        //    SendActiveVote(player);
        //}

        //public override void SendClearMsg(BasePlayer player)
        //{
        //    var msg = new GameMsg_SvVoteClearOptions();
        //    Server.SendPackMsg(msg, MsgFlags.Vital, player.ClientId);
        //}

        public override void SendVotes(BasePlayer player)
        {
            var msg = new MsgPacker((int)GameMessage.ServerVoteOptionListAdd, false);
            // num options
            msg.AddInt(20);

            for (var i = 0; i < 20; i++)
            {
                msg.AddString("Test vote");
            }

            Server.SendMsg(msg, MsgFlags.Vital, player.ClientId);
        }

        //public override void SendActiveVote(BasePlayer player)
        //{
        //    // if (vote close time != 0)
        //}

        public override void Tick()
        {
        }

        public override void CallVote(GameMsg_ClCallVote message, BasePlayer player)
        {
            if (message.Force && !Server.IsAuthed(player.ClientId))
                return;

            if (ActiveVote != null || player.Team == Team.Spectators ||
                Config["SvSpamprotection"] && (
                    LastVoteTry[player.ClientId] + Server.TickSpeed * 3 > Server.Tick ||
                    LastVoteCall[player.ClientId] + Server.TickSpeed * 60 > Server.Tick))
            {
                return;
            }

            LastVoteTry[player.ClientId] = Server.Tick;

            var reason = string.IsNullOrEmpty(message.Reason) ? message.Reason : "No reason given";

            if (message.VoteType == "option")
            {

            }
            else if (message.VoteType == "kick")
            {

            }
            else if (message.VoteType == "spectate")
            {

            }
            else
            {
                return; // unknown type
            }

            // call vote
        }
    }
}