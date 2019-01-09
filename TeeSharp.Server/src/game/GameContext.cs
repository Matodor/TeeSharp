using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Common.Snapshots;
using TeeSharp.Core;
using TeeSharp.Network;

namespace TeeSharp.Server.Game
{
    public class GameContext : BaseGameContext
    {
        public override string GameVersion { get; } = "0.7.2";
        public override string NetVersion { get; } = "0.7";
        public override string ReleaseVersion { get; } = "0.7.2";

        public override void OnInit()
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

            Votes.Init();
            MapLayers.Init(Server.CurrentMap);
            MapCollision.Init(MapLayers);
            Players = new BasePlayer[Server.MaxClients];

            GameMsgUnpacker.MaxClients = Players.Length;
            
            GameController = new GameController(); // TODO
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
            OnPlayerLeave(Players[clientId], reason);

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

            OnPlayerEnter(Players[clientId]);
        }

        protected override void ServerOnPlayerReady(int clientId)
        {
            Players[clientId] = Kernel.Get<BasePlayer>();
            Players[clientId].Init(clientId, false);

            SendMotd(clientId);
            SendSettings(clientId);

            OnPlayerReady(Players[clientId]);
        }

        public override void RegisterConsoleCommands()
        {
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

        public override void CreateExplosion(Vector2 pos, int owner, Weapon weapon, int damage)
        {
            //var e = Events.Create<SnapshotEventExplosion>();
            //if (e != null)
            //    e.Position = pos;

            //if (noDamage)
            //    return;

            //const float radius = 135.0f;
            //const float innerRadius = 48.0f;

            //var characters = World.FindEntities<Character>(pos, radius);
            //foreach (var character in characters)
            //{
            //    var diff = character.Position - pos;
            //    var forceDir = new Vector2(0, 1);
            //    var l = diff.Length;

            //    if (l > 0)
            //        forceDir = diff.Normalized;
            //    l = 1 - System.Math.Clamp((l - innerRadius) / (radius - innerRadius), 0f, 1f);
            //    var dmg = (int) (6 * l);

            //    if (dmg != 0)
            //        character.TakeDamage(forceDir * dmg * 2, dmg, owner, weapon);
            //}
        }

        public override void CreatePlayerSpawn(Vector2 pos)
        {
            Events.Create<SnapshotEventSpawn>(pos);
        }

        public override void CreateDeath(Vector2 pos, int clientId)
        {
            //var e = Events.Create<SnapshotEventDeath>();
            //if (e == null)
            //    return;

            //e.ClientId = clientId;
            //e.Position = pos;
        }

        public override void CreateDamageInd(Vector2 pos, float a, int amount)
        {
            //a = 3 * 3.14159f / 2 + a;
            //var s = a - System.Math.PI / 3;
            //var e = a + System.Math.PI / 3;

            //for (var i = 0; i < amount; i++)
            //{
            //    var f = Common.MathHelper.Mix(s, e, (float) (i + 1) / (amount + 2));
            //    var @event = Events.Create<SnapshotEventDamage>();
            //    if (@event == null)
            //        continue;

            //    @event.Position = pos;
            //    @event.Angle = (int)(f * 256.0f);
            //}
        }

        public override void CreateHammerHit(Vector2 pos)
        {
            //var e = Events.Create<SnapshotEventHammerHit>();
            //if (e == null)
            //    return;

            //e.Position = pos;
        }

        public override void CreateSound(Vector2 pos, Sound sound, int mask = -1)
        {
            //if (sound < 0 || sound >= Sound.NumSounds)
            //    return;

            //var e = Events.Create<SnapshotEventSoundWorld>();
            //if (e == null)
            //    return;

            //e.Position = pos;
            //e.Sound = sound;
        }

        public override void CreaetSoundGlobal(Sound sound, int targetId = -1)
        {
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
            //Server.SendPackMsg(new GameMsg_SvBroadcast
            //{
            //    Message = msg
            //}, MsgFlags.Vital, clientId);
        }

        public override void SendWeaponPickup(int clientId, Weapon weapon)
        {
            //Server.SendPackMsg(new GameMsg_SvWeaponPickup
            //{
            //    Weapon = weapon
            //}, MsgFlags.Vital, clientId);
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
                TeamLock = false, // TODO
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