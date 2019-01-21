using System.Collections.Generic;
using System.Linq;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Network.Extensions;

namespace TeeSharp.Server.Game
{
    public class Votes : BaseVotes
    {
        public override void Init()
        {
            GameContext = Kernel.Get<BaseGameContext>();
            GameContext.PlayerEnter += OnPlayerEnter;
            GameContext.PlayerLeave += OnPlayerLeave;

            Config = Kernel.Get<BaseConfig>();
            Server = Kernel.Get<BaseServer>();
            Console = Kernel.Get<BaseGameConsole>();

            VoteOptions = new Dictionary<string, VoteOption>();
            ActiveVote = null;
            PlayersVoteInfo = new PlayerVoteInfo[Server.MaxClients];
        }

        protected override void OnPlayerLeave(BasePlayer player, string reason)
        {
            PlayersVoteInfo[player.ClientId].Vote = 0;
            PlayersVoteInfo[player.ClientId].LastVoteCall = 0;
            PlayersVoteInfo[player.ClientId].LastVoteTry = 0;
        }

        protected override void OnPlayerEnter(BasePlayer player)
        {
            PlayersVoteInfo[player.ClientId].LastVoteCall = Server.Tick;
            ClearOptions(player);
            SendVotes(player);
        }

        public override void ClearOptions(BasePlayer player)
        {
            Server.SendPackMsg(new GameMsg_SvVoteClearOptions(), MsgFlags.Vital, player.ClientId);
        }

        public override void SendVoteSet(Vote type, BasePlayer player)
        {
            GameMsg_SvVoteSet msg;
            if (type == Vote.EndFail || type == Vote.EndAbort || type == Vote.EndPass)
            {
                msg = new GameMsg_SvVoteSet()
                {
                    ClientID = ActiveVote.CallerId,
                    Description = string.Empty,
                    VoteType = type,
                    Reason = string.Empty,
                    Timeout = 0
                };
            }
            else
            {
                msg = new GameMsg_SvVoteSet()
                {
                    ClientID = ActiveVote.CallerId,
                    Description = ActiveVote.Description,
                    VoteType = type,
                    Reason = ActiveVote.Reason,
                    Timeout = (ActiveVote.CloseTick - Server.Tick) / Server.TickSpeed
                };
            }

            Server.SendPackMsg(msg, MsgFlags.Vital, player?.ClientId ?? -1);
        }

        public override void SendVoteStatus(BasePlayer player)
        {
            var msg = new GameMsg_SvVoteStatus()
            {
                No = ActiveVote.VotesNo,
                Yes = ActiveVote.VotesYes,
                Total = ActiveVote.VotesTotal,
                Pass = ActiveVote.VotesTotal - (ActiveVote.VotesYes + ActiveVote.VotesNo)
            };

            Server.SendPackMsg(msg, MsgFlags.Vital, player?.ClientId ?? -1);
        }

        public override void SendVotes(BasePlayer player)
        {
            const int VotesPerMsg = 20;

            KeyValuePair<string, VoteOption>[] options;
            var skip = 0;

            do
            {
                options = VoteOptions.Skip(skip).Take(VotesPerMsg).ToArray();
                skip += options.Length;

                var msg = new MsgPacker((int)GameMessage.ServerVoteOptionListAdd, false);
                msg.AddInt(options.Length);

                for (var i = 0; i < options.Length; i++)
                    msg.AddString(options[i].Value.Description);

                Server.SendMsg(msg, MsgFlags.Vital, player.ClientId);
            } while (options.Length >= VotesPerMsg);
        }

        public override void Tick()
        {
        }

        public override bool AddVote(string description, string command)
        {
            if (string.IsNullOrEmpty(description) || description.Length > VoteOption.MaxDescription)
            {
                Console.Print(OutputLevel.Standard, "votes", $"skipped invalid option '{description}'");
                return false;
            }

            if (string.IsNullOrEmpty(command) || command.Length > VoteOption.MaxCommand || !Console.IsLineValid(command))
            {
                Console.Print(OutputLevel.Standard, "votes", $"skipped invalid command '{command}'");
                return false;
            }

            if (ContainsVote(description))
            {
                Console.Print(OutputLevel.Standard, "votes", $"option '{description}' already exists");
                return false;
            }

            var voteOption = new VoteOption()
            {
                Description = description,
                Command = command
            };

            VoteOptions.Add(voteOption.Description, voteOption);
            Console.Print(OutputLevel.Standard, "votes", $"added option '{voteOption.Description}' '{voteOption.Command}'");

            Server.SendPackMsg(new GameMsg_SvVoteOptionAdd
            {
                Description = description
            }, MsgFlags.Vital, -1);

            return true;
        }

        public override bool ContainsVote(string description)
        {
            return VoteOptions.ContainsKey(description);
        }

        public override void StartVote(ActiveVote vote)
        {
            ActiveVote = vote;
            SendVoteSet(vote.Type, null);
            SendVoteStatus(null);
        }

        public override void ClientVote(GameMsg_ClVote message, BasePlayer player)
        {
            if (ActiveVote == null)
                return;

            if (PlayersVoteInfo[player.ClientId].Vote == 0)
            {
                if (message.Vote == 0)
                    return;

                PlayersVoteInfo[player.ClientId].Vote = message.Vote;
                ActiveVote.VotesTotal++;

                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (player.ClientId == i ||
                        GameContext.Players[i] == null ||
                        GameContext.Players[i].Team == Team.Spectators || !Server.ClientEndPoint(i)
                            .Compare(Server.ClientEndPoint(player.ClientId), false))
                    {
                        continue;
                    }

                    if (PlayersVoteInfo[i].Vote != 0)
                        return;
                }

                if (message.Vote > 0)
                    ActiveVote.VotesYes++;
                else if (message.Vote < 0)
                    ActiveVote.VotesNo++;

                SendVoteStatus(null);
                CheckVoteStatus();
            }
            else if (ActiveVote.CallerId == player.ClientId)
            {

            }
        }

        protected override void CheckVoteStatus()
        {
            if (ActiveVote == null)
                return;

            if (ActiveVote.VotesYes >= ActiveVote.VotesTotal / 2 + 1)
            {
                EndVote(Vote.EndPass);
            }
            else if (ActiveVote.VotesNo >= (ActiveVote.VotesTotal + 1) / 2)
            {
                EndVote(Vote.EndFail);
            }
        }

        public override void EndVote(Vote type)
        {
            if (type == Vote.EndPass)
            {
                Console.ExecuteLine(ActiveVote.Command, BaseServerClient.AuthedAdmin, -1);

                if (ActiveVote.CallerId != -1 && GameContext.Players[ActiveVote.CallerId] != null)
                    PlayersVoteInfo[ActiveVote.CallerId].LastVoteCall = 0;
            }

            SendVoteSet(type, null);
            ActiveVote = null;
        }

        public override void CallVote(GameMsg_ClCallVote message, BasePlayer player)
        {
            if (message.Force && !Server.IsAuthed(player.ClientId))
                return;

            if (Config["SvSpamprotection"] && PlayersVoteInfo[player.ClientId].LastVoteTry + Server.TickSpeed * 3 > Server.Tick)
                return;

            PlayersVoteInfo[player.ClientId].LastVoteTry = Server.Tick;

            if (ActiveVote != null)
            {
                GameContext.SendChat(-1, ChatMode.All, player.ClientId, "Wait for the current vote to end");
                return;
            }

            if (player.Team == Team.Spectators)
            {
                GameContext.SendChat(-1, ChatMode.All, player.ClientId, "Wait for the current vote to end");
                return;
            }

            // TODO
            //var timeRemaning = 60 - (Server.Tick - PlayerLastVoteCall[player.ClientId]) / Server.TickSpeed;
            //if (Config["SvSpamprotection"] && timeRemaning > 0)
            //{
            //    GameContext.SendChat(-1, ChatMode.All, player.ClientId,
            //        $"Wait '{timeRemaning}' seconds for start new vote");
            //    return;
            //}

            PlayersVoteInfo[player.ClientId].LastVoteCall = Server.Tick;
            var reason = string.IsNullOrEmpty(message.Reason) ? "No reason given" : message.Reason;
            string description; 
            string command;
            Vote voteType;

            if (message.VoteType == "option")
            {
                if (!VoteOptions.TryGetValue(message.Value, out var voteOption))
                    return;

                voteType = Vote.StartOption;
                description = voteOption.Description;
                command = voteOption.Command;
            }
            else if (message.VoteType == "kick")
            {
                voteType = Vote.StartKick;
                description = string.Empty;
                command = string.Empty;
            }
            else if (message.VoteType == "spectate")
            {
                voteType = Vote.StartSpectator;
                description = string.Empty;
                command = string.Empty;
            }
            else
            {
                return; // unknown type
            }

            if (message.Force)
            {

            }
            else
            {
                StartVote(new ActiveVote
                {
                    CallerId = player.ClientId,
                    Description = description,
                    Type = voteType,
                    Reason = reason,
                    CloseTick = Server.Tick + Server.TickSpeed * 25,
                    Command = command
                });

                PlayersVoteInfo[player.ClientId].Vote = 1;
                PlayersVoteInfo[player.ClientId].LastVoteCall = Server.Tick;
                ActiveVote.VotesTotal++;
                ActiveVote.VotesYes++;

                CheckVoteStatus();
            }
            // call vote
        }
    }
}