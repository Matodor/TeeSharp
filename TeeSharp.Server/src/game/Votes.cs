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
            Server = Kernel.Get<BaseServer>();
        }

        public override void SendClearMsg(int clientId)
        {
            var msg = new GameMsg_SvVoteClearOptions();
            Server.SendPackMsg(msg, MsgFlags.Vital, clientId);
        }

        public override void SendVotes(int clientId)
        {
            var msg = new MsgPacker((int) GameMessage.ServerVoteOptionListAdd, false);
            // num options
            msg.AddInt(20);

            // TODO
            for (var i = 0; i < 20; i++)
            {
                msg.AddString("Test vote");
            }

            Server.SendMsg(msg, MsgFlags.Vital, clientId);
        }

        public override void SendActiveVote(int clientId)
        {
            // if (vote close time != 0)
        }

        public override void PlayerConnected(int clientId)
        {
            
        }

        public override void PlayerDisconnected(int clientId)
        {
            
        }

        public override void PlayerChangeTeam(int clientId, Team prev, Team next)
        {
            
        }

        public override void Tick()
        {
        }
    }
}