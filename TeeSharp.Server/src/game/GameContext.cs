using System;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Common.Snapshots;
using TeeSharp.Core;
using TeeSharp.Core.Extensions;
using TeeSharp.Network;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
    public class GameContext : BaseGameContext
    {
        public override event PlayerEvent PlayerReady;
        public override event PlayerEvent PlayerEnter;
        public override event PlayerLeaveEvent PlayerLeave;

        public override string GameVersion { get; } = "0.7.2";
        public override string NetVersion { get; } = "0.7";
        public override string ReleaseVersion { get; } = "0.7.2";

        public override void BeforeInit()
        {
            Votes = Kernel.Get<BaseVotes>();
            Events = Kernel.Get<BaseEvents>();
            Server = Kernel.Get<BaseServer>();
            MapLayers = Kernel.Get<BaseMapLayers>();
            GameMsgUnpacker = Kernel.Get<BaseGameMsgUnpacker>();
            MapCollision = Kernel.Get<BaseMapCollision>();
            Config = Kernel.Get<BaseConfig>();
            Console = Kernel.Get<BaseGameConsole>();
            Tuning = Kernel.Get<BaseTuningParams>();
            World = Kernel.Get<BaseGameWorld>();
        }

        public override void Init()
        {
            Votes.Init();
            Events.Init();
            MapLayers.Init(Server.CurrentMap);
            MapCollision.Init(MapLayers);
            Players = new BasePlayer[Server.MaxClients];

            GameMsgUnpacker.MaxClients = Players.Length;

            GameController = Server.GameController(Config["SvGametype"]);
            GameController.Init();

            Server.PlayerReady += ServerOnPlayerReady;
            Server.PlayerEnter += ServerOnPlayerEnter;
            Server.PlayerDisconnected += ServerOnPlayerDisconnected;

            for (var y = 0; y < MapLayers.GameLayer.Height; y++)
            {
                for (var x = 0; x < MapLayers.GameLayer.Width; x++)
                {
                    var tile = MapCollision.GetTile(y * MapLayers.GameLayer.Width + x);
                    var pos = new Vector2(x * 32.0f + 16.0f, y * 32.0f + 16.0f);
                    
                    GameController.OnEntity(tile, pos);
                }
            }

            CheckPureTuning();
        }

        protected override void ServerOnPlayerDisconnected(int clientId, string reason)
        {
            PlayerLeave?.Invoke(Players[clientId], reason);

            if (Server.ClientInGame(clientId))
            {
                if (false) // TODO DEMO
                {
                    Server.SendPackMsg(new GameMsg_DeClientLeave()
                    {
                        Name = Server.ClientName(clientId),
                        Reason = reason,
                        ClientId = clientId,
                    }, MsgFlags.NoSend, -1);
                }

                Server.SendPackMsg(new GameMsg_SvClientDrop()
                    {
                        ClientID = clientId,
                        Reason = reason,
                        Silent = Config["SvSilentSpectatorMode"] && Players[clientId].Team == Team.Spectators,
                    }, MsgFlags.Vital | MsgFlags.NoRecord, -1);
            }

            Players[clientId].OnPlayerLeave(reason);
            Players[clientId] = null;
        }

        protected override void ServerOnPlayerEnter(int clientId)
        {
            var clientInfo = ClientInfo(clientId);
            if (Config["SvSilentSpectatorMode"] && Players[clientId].Team == Team.Spectators)
                clientInfo.Silent = true;

            for (var i = 0; i < Players.Length; i++)
            {
                if (i == clientId || Players[i] == null || !Server.ClientInGame(i) && !Players[i].IsDummy)
                    continue;

                if (Server.ClientInGame(i))
                    Server.SendPackMsg(clientInfo, MsgFlags.Vital | MsgFlags.NoRecord, i);

                Server.SendPackMsg(ClientInfo(i), MsgFlags.Vital | MsgFlags.NoRecord, clientId);
            }

            clientInfo.Local = true;
            Server.SendPackMsg(clientInfo, MsgFlags.Vital | MsgFlags.NoRecord, clientId);

            if (false) // TODO DEMO 
            {
                var msg = new GameMsg_DeClientEnter()
                {
                    Name = clientInfo.Name,
                    Team = clientInfo.Team,
                    ClientId = clientId
                };
                Server.SendPackMsg(msg, MsgFlags.NoSend, -1);
            }

            PlayerEnter?.Invoke(Players[clientId]);
        }

        protected override void ServerOnPlayerReady(int clientId)
        {
            Players[clientId] = Kernel.Get<BasePlayer>();
            Players[clientId].Init(clientId, false);

            SendMotd(clientId);
            SendSettings(clientId);
            PlayerReady?.Invoke(Players[clientId]);
        }

        public override void RegisterCommandsUpdates()
        {
            Console["sv_motd"].Executed += ConsoleMotdUpdated;

            Console["sv_vote_kick"].Executed += ConsoleSettingsUpdated;
            Console["sv_vote_kick_min"].Executed += ConsoleSettingsUpdated;
            Console["sv_vote_spectate"].Executed += ConsoleSettingsUpdated;
            Console["sv_teambalance_time"].Executed += ConsoleSettingsUpdated;
            Console["sv_player_slots"].Executed += ConsoleSettingsUpdated;

            Console["sv_scorelimit"].Executed += ConsoleGameInfoUpdated;
            Console["sv_timelimit"].Executed += ConsoleGameInfoUpdated;
            Console["sv_matches_per_map"].Executed += ConsoleGameInfoUpdated;
        }

        protected virtual void ConsoleSettingsUpdated(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleMotdUpdated(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleGameInfoUpdated(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        public override void RegisterConsoleCommands()
        {
            Console.AddCommand("tune", "si", "Tune variable to value", ConfigFlags.Server, ConsoleTune);
            Console.AddCommand("tune_reset", string.Empty, "Reset tuning", ConfigFlags.Server, ConsoleTuneReset);
            Console.AddCommand("tune_dump", string.Empty, "Dump tuning", ConfigFlags.Server, ConsoleTuneDump);

            Console.AddCommand("pause", "?i", "Pause/unpause game", ConfigFlags.Server | ConfigFlags.Store, ConsolePause);
            Console.AddCommand("change_map", "r", "Change map", ConfigFlags.Server | ConfigFlags.Store, ConsoleChangeMap);
            Console.AddCommand("restart", "?i", "Restart in x seconds (0 == abort)", ConfigFlags.Server | ConfigFlags.Store, ConsoleRestart);
            Console.AddCommand("say", "r", "Say in chat", ConfigFlags.Server, ConsoleSay);
            Console.AddCommand("broadcast", "r", "Broadcast message", ConfigFlags.Server, ConsoleBroadcast);
            Console.AddCommand("set_team", "ii?i", "Set team of player to team", ConfigFlags.Server, ConsoleSetTeam);
            Console.AddCommand("set_team_all", "i", "Set team of all players to team", ConfigFlags.Server, ConsoleSetTeamAll);
            Console.AddCommand("swap_teams", string.Empty, "Swap the current teams", ConfigFlags.Server, ConsoleSwapTeams);
            Console.AddCommand("shuffle_teams", string.Empty, "Shuffle the current teams", ConfigFlags.Server, ConsoleShuffleTeams);
            Console.AddCommand("lock_teams", string.Empty, "Lock/unlock teams", ConfigFlags.Server, ConsoleLockTeams);
            Console.AddCommand("force_teambalance", string.Empty, "Force team balance", ConfigFlags.Server, ConsoleForceTeamBalance);

            Console.AddCommand("add_vote", "sr", "Add a voting option", ConfigFlags.Server, ConsoleAddVote);
            Console.AddCommand("remove_vote", "s", "Remove a voting option", ConfigFlags.Server, ConsoleRemoveVote);
            Console.AddCommand("clear_votes", string.Empty, "Clears the voting options", ConfigFlags.Server, ConsoleClearVotes);
            Console.AddCommand("vote", "r", "Force a vote to yes/no", ConfigFlags.Server, ConsoleVote);
        }

        protected virtual void ConsoleVote(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleClearVotes(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleRemoveVote(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleAddVote(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleForceTeamBalance(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleLockTeams(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleShuffleTeams(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleSwapTeams(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleSetTeamAll(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleSetTeam(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleBroadcast(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleSay(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleRestart(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleChangeMap(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsolePause(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleTuneDump(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleTuneReset(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleTune(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }
        
        public override bool IsClientSpectator(int clientId)
        {
            return Players[clientId] != null && Players[clientId].Team == Team.Spectators;
        }

        public override bool IsClientReady(int clientId)
        {
            return Players[clientId] != null && Players[clientId].IsReadyToEnter;
        }

        public override bool IsClientPlayer(int clientId)
        {
            return Players[clientId] != null && Players[clientId].Team != Team.Spectators;
        }

        public override void CreateExplosion(Vector2 position, int owner, Weapon weapon, int maxDamage)
        {
            Events.Create<SnapshotEventExplosion>(position);

            const float innerRadius = 48f;
            var radius = ServerData.Explosion.Radius;
            var maxForce = ServerData.Explosion.MaxForce;

            foreach (var character in Character.Entities.Find(position, radius))
            {
                var diff = character.Position - position;
                var force = new Vector2(0, maxForce);
                var length = diff.Length;

                if (Math.Abs(length) > 0.0001f)
                    force = diff.Normalized * maxForce;

                var factor = 1 - Math.Clamp((length - innerRadius) / (radius - innerRadius), 0f, 1f);
                if ((int) (factor * maxDamage) != 0)
                    character.TakeDamage(force * factor, diff * -1, (int) (factor * maxDamage), owner, weapon);
            }
        }

        public override void CreatePlayerSpawn(Vector2 pos)
        {
            Events.Create<SnapshotEventSpawn>(pos);
        }

        public override void CreateDeath(Vector2 position, int clientId)
        {
            var e = Events.Create<SnapshotEventDeath>(position);
            if (e == null)
                return;

            e.ClientId = clientId;
        }

        public override void CreateDamageIndicator(Vector2 pos, Vector2 source, 
            int clientId, int healthAmount, int armorAmount, bool self)
        {
            var e = Events.Create<SnapshotEventDamage>(pos);
            if (e == null)
                return;

            e.ClientId = clientId;
            e.Angle = (int) (MathHelper.Angle(source) * 256f);
            e.ArmorAmount = armorAmount;
            e.HealthAmount = healthAmount;
            e.IsSelf = self;
        }

        public override void CreateHammerHit(Vector2 pos)
        {
            Events.Create<SnapshotEventHammerHit>(pos);
        }

        public override void CreateSound(Vector2 position, Sound sound, int mask = -1)
        {
            if (sound < 0 || sound >= Sound.NumSounds)
                return;

            var e = Events.Create<SnapshotEventSoundWorld>(position, mask);
            if (e == null)
                return;

            e.Sound = sound;
        }

        public override void CreateSoundGlobal(Sound sound, int targetId = -1)
        {
            throw new NotImplementedException();
            //if (sound < 0 || sound >= Sound.NumSounds)
            //    return;

            //var msg = new GameMsg_SvSoundGlobal
            //{
            //    Sound = sound
            //};

            //if (targetId == -2)
            //    Server.SendPackMsg(msg, MsgFlags.NoSend, -1);
            //else
            //{
            //    var flags = MsgFlags.Vital;
            //    if (targetId != -1)
            //        flags |= MsgFlags.NoRecord;
            //    Server.SendPackMsg(msg, flags, targetId);
            //}
        }

        public override void CheckPureTuning()
        {
            //if (GameController == null)
            //    return;

            //if (new[] {"DM", "TDM", "CTF"}.Contains(GameController.GameType))
            //{
            //    var error = false;
            //    foreach (var pair in Tuning)
            //    {
            //        if (pair.Value.Value != pair.Value.DefaultValue)
            //        {
            //            error = true;
            //            break;
            //        }
            //    }

            //    if (error)
            //    {
            //        Tuning.Reset();
            //        Console.Print(OutputLevel.Standard, "server", "resetting tuning due to pure server");
            //    }
            //}
        }

        public override void SendTuningParams(int clientId)
        {
            CheckPureTuning();

            var msg = new MsgPacker((int) GameMessage.ServerTuneParams, false);
            foreach (var pair in Tuning)
                msg.AddInt(pair.Value.Value);
            Server.SendMsg(msg, MsgFlags.Vital, clientId);
        }

        public override void SendBroadcast(int clientId, string msg)
        {
            Server.SendPackMsg(new GameMsg_SvBroadcast
            {
                Message = msg
            }, MsgFlags.Vital, clientId);
        }

        public override void SendWeaponPickup(int clientId, Weapon weapon)
        {
            Server.SendPackMsg(new GameMsg_SvWeaponPickup
            {
                Weapon = weapon
            }, MsgFlags.Vital, clientId);
        }

        public override void SendEmoticon(int clientId, Emoticon emote)
        {
            Server.SendPackMsg(new GameMsg_SvEmoticon()
            {
                ClientId = clientId,
                Emoticon = emote
            }, MsgFlags.Vital, -1);
        }

        public override void SendChatTarget(int clientId, string msg)
        {
            //Server.SendPackMsg(new GameMsg_SvChat
            //{
            //    IsTeam = false,
            //    ClientId = -1,
            //    Message = msg
            //}, MsgFlags.Vital, clientId);
        }

        public override void SendChat(int from, ChatMode mode, int target, string message)
        {
            if (mode == ChatMode.None)
                return;

            var log = from < 0 ? $"*** {message}" : $"{@from}:{Server.ClientName(@from)}: {message}";
            Console.Print(OutputLevel.AddInfo, mode.ToString(), log);

            var msg = new GameMsg_SvChat()
            {
                ChatMode = mode,
                ClientId = from,
                TargetId = -1,
                Message = message,
            };

            if (mode == ChatMode.All)
                Server.SendPackMsg(msg, MsgFlags.Vital, -1);
            else if (mode == ChatMode.Team)
            {
                Server.SendPackMsg(msg, MsgFlags.Vital | MsgFlags.NoSend, -1);

                var team = Players[from].Team;

                for (var i = 0; i < Players.Length; i++)
                {
                    if (Players[i] != null && Players[i].Team == team)
                        Server.SendPackMsg(msg, MsgFlags.Vital | MsgFlags.NoRecord, i);
                }
            }
            else
            {
                msg.TargetId = target;
                Server.SendPackMsg(msg, MsgFlags.Vital, from);
                Server.SendPackMsg(msg, MsgFlags.Vital, target);
            }
        }

        public override void OnTick()
        {
            CheckPureTuning();

            World.Tick();
            GameController.Tick();

            for (var i = 0; i < Players.Length; i++)
            {
                if (Players[i] == null)
                    continue;
                
                Players[i].Tick();
                Players[i].PostTick();
            }

            Votes.Tick();
        }

        public override void OnShutdown()
        {
            throw new System.NotImplementedException();
        }

        public override void OnMessage(GameMessage msg, UnPacker unPacker, int clientId)
        {
            if (!GameMsgUnpacker.UnpackMessage(msg, unPacker, out var message, out string failedOn))
            {
                Console.Print(OutputLevel.Debug, "server", $"dropped message={msg} failed on={failedOn}");
                return;
            }

            var player = Players[clientId];

            if (Server.ClientInGame(clientId))
            {
                switch (msg)
                {
                    case GameMessage.ClientSay:
                        OnMsgClientSay(player, (GameMsg_ClSay) message);
                        break;
                    case GameMessage.ClientSetTeam:
                        OnMsgClientSetTeam(player, (GameMsg_ClSetTeam) message);
                        break;
                    case GameMessage.ClientSetSpectatorMode:
                        OnMsgClientSetSpectatorMode(player, (GameMsg_ClSetSpectatorMode) message);
                        break;
                    case GameMessage.ClientStartInfo:
                        break;
                    case GameMessage.ClientKill:
                        OnMsgClientKill(player, (GameMsg_ClKill) message);
                        break;
                    case GameMessage.ClientReadyChange:
                        OnMsgClientReadyChange(player, (GameMsg_ClReadyChange) message);
                        break;
                    case GameMessage.ClientEmoticon:
                        OnMsgClientEmoticon(player, (GameMsg_ClEmoticon) message);
                        break;
                    case GameMessage.ClientVote:
                        break;
                    case GameMessage.ClientCallVote:
                        break;
                }
            }
            else if (msg == GameMessage.ClientStartInfo)
            {
                OnMsgClientStartInfo(player, (GameMsg_ClStartInfo) message);
            }
        }

        protected override void OnMsgClientSetSpectatorMode(BasePlayer player, GameMsg_ClSetSpectatorMode message)
        {
            if (Config["SvSpamprotection"] && player.LastSetSpectatorMode + Server.TickSpeed > Server.Tick)
                return;

            player.LastSetSpectatorMode = Server.Tick;
            if (!player.SetSpectatorID(message.SpectatorMode, message.SpectatorId))
                SendGameplayMessage(player.ClientId, GameplayMessage.SpectatorInvalidId);
        }

        protected override void OnMsgClientReadyChange(BasePlayer player, GameMsg_ClReadyChange message)
        {
            if (player.LastReadyChangeTick + Server.TickSpeed > Server.Tick)
                return;

            player.LastReadyChangeTick = Server.Tick;
            GameController.OnPlayerReadyChange(player);
        }

        protected override void OnMsgClientKill(BasePlayer player, GameMsg_ClKill message)
        {
            if (GameController.CanSelfKill(player) && player.LastKillTick + Server.TickSpeed * 3 > Server.Tick)
                return;

            player.LastKillTick = Server.Tick;
            player.KillCharacter(BasePlayer.WeaponSelf);
        }

        protected override void OnMsgClientEmoticon(BasePlayer player, GameMsg_ClEmoticon message)
        {
            if (Config["SvSpamprotection"] && player.LastEmoteTick + Server.TickSpeed * 3 > Server.Tick)
                return;

            player.LastEmoteTick = Server.Tick;
            SendEmoticon(player.ClientId, message.Emoticon);
        }

        protected override void OnMsgClientSetTeam(BasePlayer player, GameMsg_ClSetTeam message)
        {
            if (!GameController.IsTeamChangeAllowed(player))
                return;

            if (player.Team == message.Team)
                return;

            if (Config["SvSpamprotection"] && player.LastSetTeamTick + Server.TickSpeed * 3 > Server.Tick)
                return;

            if (message.Team != Team.Spectators && LockTeams || player.TeamChangeTick > Server.Tick)
                return;

            player.LastSetTeamTick = Server.Tick;

            if (GameController.CanJoinTeam(player, message.Team) &&
                GameController.CanChangeTeam(player, message.Team))
            {
                player.SetTeam(message.Team);
            }
        }

        protected override void OnMsgClientSay(BasePlayer player, GameMsg_ClSay message)
        {
            if (string.IsNullOrEmpty(message.Message))
                return;

            if (Config["SvSpamprotection"] && player.LastChatTick + Server.TickSpeed > Server.Tick)
                return;

            message.Message = message.Message.Limit(128);

            if (Config["SvTournamentMode"] == 2 && player.Team == Team.Spectators &&
                GameController.GameRunning && Server.IsAuthed(player.ClientId))
            {
                if (message.ChatMode != ChatMode.Whisper)
                    message.ChatMode = ChatMode.Team;
                else if (Players[message.TargetId] != null && Players[message.TargetId].Team != Team.Spectators)
                    message.ChatMode = ChatMode.None;
            }

            player.LastChatTick = Server.Tick;
            GameController.OnPlayerChat(player, message, out var isSend);

            if (isSend)
            {
                SendChat(player.ClientId, message.ChatMode, message.TargetId, message.Message);
            }
        }

        protected override void OnMsgClientStartInfo(BasePlayer player, GameMsg_ClStartInfo startInfo)
        {
            if (player.IsReadyToEnter)
                return;

            Server.ClientName(player.ClientId, startInfo.Name.Limit(BaseServerClient.MaxNameLength));
            Server.ClientClan(player.ClientId, startInfo.Clan.Limit(BaseServerClient.MaxClanLength));
            Server.ClientCountry(player.ClientId, startInfo.Country);

            for (var i = SkinPart.Body; i < SkinPart.NumParts; i++)
            {
                player.TeeInfo[i].Name = startInfo.SkinPartNames[(int)i];
                player.TeeInfo[i].Color = startInfo.SkinPartColors[(int)i];
                player.TeeInfo[i].UseCustomColor = startInfo.UseCustomColors[(int)i];
            }

            player.OnChangeInfo();

            GameController.OnPlayerInfoChange(player);

            Votes.SendClearMsg(player);
            Votes.SendVotes(player);

            SendTuningParams(player.ClientId);
            player.ReadyToEnter();
        }

        public override void OnBeforeSnapshot()
        {
            World.BeforeSnapshot();
        }

        public override void OnAfterSnapshots()
        {
            World.AfterSnapshot();
            Events.Clear();
        }

        public override void OnSnapshot(int snappingId)
        {
            // TODO DEMO TUNING PARAMS 
            {

            }

            World.OnSnapshot(snappingId);
            GameController.OnSnapshot(snappingId, out _);
            Events.OnSnapshot(snappingId);

            for (var i = 0; i < Players.Length; i++)
            {
                if (Players[i] == null)
                    continue;
                Players[i].OnSnapshot(snappingId, out _, out _, out _);
            }
        }

        protected override GameMsg_SvClientInfo ClientInfo(int clientId)
        {
            var clientInfo = new GameMsg_SvClientInfo()
            {
                ClientID = clientId,
                Local = false,
                Team = Players[clientId].Team,
                Name = Server.ClientName(clientId),
                Clan = Server.ClientClan(clientId),
                Country = Server.ClientCountry(clientId),
                Silent = false,
            };

            for (var i = SkinPart.Body; i < SkinPart.NumParts; i++)
            {
                clientInfo.SkinPartNames[(int)i] = Players[clientId].TeeInfo[i].Name;
                clientInfo.SkinPartColors[(int)i] = Players[clientId].TeeInfo[i].Color;
                clientInfo.UseCustomColors[(int)i] = Players[clientId].TeeInfo[i].UseCustomColor;
            }

            return clientInfo;
        }

        public override void OnClientPredictedInput(int clientId, int[] input)
        {
            if (!World.Paused)
            {
                var playerInput = BaseSnapshotItem.FromArray<SnapshotPlayerInput>(input);
                if (playerInput.IsValid())
                    Players[clientId].OnPredictedInput(playerInput);
                else 
                    Console.Print(OutputLevel.Debug, "server", "SnapshotPlayerInput not valid");
            }
        }

        public override void OnClientDirectInput(int clientId, int[] input)
        {
            var playerInput = BaseSnapshotItem.FromArray<SnapshotPlayerInput>(input);
            if (playerInput.IsValid())
                Players[clientId].OnDirectInput(playerInput);
            else
                Console.Print(OutputLevel.Debug, "server", "SnapshotPlayerInput not valid");
        }

        public override void SendSettings(int clientId)
        {
            var msg = new GameMsg_SvSettings()
            {
                KickVote = Config["SvVoteKick"],
                KickMin = Config["SvVoteKickMin"],
                SpectatorsVote = Config["SvVoteSpectate"],
                TeamLock = LockTeams,
                TeamBalance = Config["SvTeambalanceTime"],
                PlayerSlots = Config["SvPlayerSlots"],
            };
            Server.SendPackMsg(msg, MsgFlags.Vital, clientId);
        }

        public override void SendGameplayMessage(int clientId, GameplayMessage message, 
            int? param1 = null, int? param2 = null, int? param3 = null)
        {
            var msg = new GameMsg_SvGameMsg()
            {
                Message = message,
                Param1 = param1,
                Param2 = param2,
                Param3 = param3
            };

            Server.SendPackMsg(msg, MsgFlags.Vital, clientId);
        }

        public override void SendMotd(int clientId)
        {
            var msg = new GameMsg_SvMotd()
            {
                Message = Config["SvMotd"]
            };
            Server.SendPackMsg(msg, MsgFlags.Vital, clientId);
        }
    }
}