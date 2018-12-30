using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game
{
    public class Votes : BaseVotes
    {
        public override void Init()
        {
            GameContext = Kernel.Get<BaseGameContext>();
            GameContext.PlayerReady += OnPlayerReady;
            Server = Kernel.Get<BaseServer>();
        }

        protected override void OnPlayerReady(BasePlayer player)
        {
            SendActiveVote(player);
        }

        public override void SendClearMsg(BasePlayer player)
        {
            var msg = new GameMsg_SvVoteClearOptions();
            Server.SendPackMsg(msg, MsgFlags.Vital, player.ClientId);
        }

        public override void SendVotes(BasePlayer player)
        {
            var msg = new MsgPacker((int) GameMessage.ServerVoteOptionListAdd, false);
            // num options
            msg.AddInt(20);

            // TODO
            for (var i = 0; i < 20; i++)
            {
                msg.AddString("Test vote");
            }

            Server.SendMsg(msg, MsgFlags.Vital, player.ClientId);
        }

        public override void SendActiveVote(BasePlayer player)
        {
            // if (vote close time != 0)
        }

        public override void Tick()
        {
        }
    }
}