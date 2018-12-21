using System.Linq;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
    public class GameContext : BaseGameContext
    {
        public override string GameVersion { get; } = "0.6";
        public override string NetVersion { get; } = "0.6";
        public override string ReleaseVersion { get; } = "0.63";

        public override void OnInit()
        {
            Votes = Kernel.Get<BaseVotes>();
            Events = Kernel.Get<BaseEvents>();
            Server = Kernel.Get<BaseServer>();
            Layers = Kernel.Get<BaseLayers>();
            GameMsgUnpacker = Kernel.Get<BaseGameMsgUnpacker>();
            Collision = Kernel.Get<BaseCollision>();
            Config = Kernel.Get<BaseConfig>();
            Console = Kernel.Get<BaseGameConsole>();
            Tuning = Kernel.Get<BaseTuningParams>();
            World = Kernel.Get<BaseGameWorld>();

            Layers.Init(Server.CurrentMap);
            Collision.Init(Layers);
            Players = new BasePlayer[Server.MaxClients];
            
            // TODO
            GameController = new GameControllerDM();

            for (var y = 0; y < Layers.GameLayer.Height; y++)
            {
                for (var x = 0; x < Layers.GameLayer.Width; x++)
                {
                    var tile = Collision.GetTileAtIndex(y * Layers.GameLayer.Width + x);
                    var pos = new Vector2(x * 32.0f + 16.0f, y * 32.0f + 16.0f);

                    if (tile.Index >= (int) MapItems.ENTITY_OFFSET)
                        GameController.OnEntity(tile.Index - (int) MapItems.ENTITY_OFFSET, pos);
                }
            }

            CheckPureTuning();
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
            return Players[clientId] != null && Players[clientId].IsReady;
        }

        public override void CreateExplosion(Vector2 pos, int owner, Weapon weapon, bool noDamage)
        {
            var e = Events.Create<SnapEvent_Explosion>();
            if (e != null)
                e.Position = pos;

            if (noDamage)
                return;

            const float radius = 135.0f;
            const float innerRadius = 48.0f;

            var characters = World.FindEntities<Character>(pos, radius);
            foreach (var character in characters)
            {
                var diff = character.Position - pos;
                var forceDir = new Vector2(0, 1);
                var l = diff.Length;

                if (l > 0)
                    forceDir = diff.Normalized;
                l = 1 - System.Math.Clamp((l - innerRadius) / (radius - innerRadius), 0f, 1f);
                var dmg = (int) (6 * l);

                if (dmg != 0)
                    character.TakeDamage(forceDir * dmg * 2, dmg, owner, weapon);
            }
        }

        public override void CreatePlayerSpawn(Vector2 pos)
        {
            var e = Events.Create<SnapEvent_Spawn>();
            if (e == null)
                return;

            e.Position = pos;
        }

        public override void CreateDeath(Vector2 pos, int clientId)
        {
            var e = Events.Create<SnapEvent_Death>();
            if (e == null)
                return;

            e.ClientId = clientId;
            e.Position = pos;
        }

        public override void CreateDamageInd(Vector2 pos, float a, int amount)
        {
            a = 3 * 3.14159f / 2 + a;
            var s = a - System.Math.PI / 3;
            var e = a + System.Math.PI / 3;

            for (var i = 0; i < amount; i++)
            {
                var f = Common.MathHelper.Mix(s, e, (float) (i + 1) / (amount + 2));
                var @event = Events.Create<SnapEvent_Damage>();
                if (@event == null)
                    continue;

                @event.Position = pos;
                @event.Angle = (int)(f * 256.0f);
            }
        }

        public override void CreateHammerHit(Vector2 pos)
        {
            var e = Events.Create<SnapEvent_HammerHit>();
            if (e == null)
                return;

            e.Position = pos;
        }

        public override void CreateSound(Vector2 pos, Sound sound, int mask = -1)
        {
            if (sound < 0 || sound >= Sound.NumSounds)
                return;

            var e = Events.Create<SnapEvent_SoundWorld>();
            if (e == null)
                return;

            e.Position = pos;
            e.Sound = sound;
        }

        public override void CreaetSoundGlobal(Sound sound, int targetId = -1)
        {
            if (sound < 0 || sound >= Sound.NumSounds)
                return;

            var msg = new GameMsg_SvSoundGlobal
            {
                Sound = sound
            };

            if (targetId == -2)
                Server.SendPackMsg(msg, MsgFlags.NoSend, -1);
            else
            {
                var flags = MsgFlags.Vital;
                if (targetId != -1)
                    flags |= MsgFlags.NoRecord;
                Server.SendPackMsg(msg, flags, targetId);
            }
        }

        public override void CheckPureTuning()
        {
            if (GameController == null)
                return;

            if (new[] {"DM", "TDM", "CTF"}.Contains(GameController.GameType))
            {
                var error = false;
                foreach (var pair in Tuning)
                {
                    if (pair.Value.Value != pair.Value.DefaultValue)
                    {
                        error = true;
                        break;
                    }
                }

                if (error)
                {
                    Tuning.Reset();
                    Console.Print(OutputLevel.STANDARD, "server", "resetting tuning due to pure server");
                }
            }
        }

        public override void SendTuningParams(int clientId)
        {
            var msg = new MsgPacker((int) GameMessage.SV_TUNEPARAMS);
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

        public override void SendChatTarget(int clientId, string msg)
        {
            Server.SendPackMsg(new GameMsg_SvChat
            {
                IsTeam = false,
                ClientId = -1,
                Message = msg
            }, MsgFlags.Vital, clientId);
        }

        public override void SendChat(int chatterClientId, bool isTeamChat, string msg)
        {
            string debug;
            if (chatterClientId >= 0 && chatterClientId < Players.Length)
                debug = $"{chatterClientId}:{Players[chatterClientId].Name} {msg}";
            else
                debug = $"*** {msg}";
            Console.Print(OutputLevel.ADDINFO, isTeamChat ? "teamchat" : "chat", debug);

            if (isTeamChat)
            {
                var p = new GameMsg_SvChat
                {
                    IsTeam = true,
                    ClientId = chatterClientId,
                    Message = msg
                };

                // pack one for the recording only
                Server.SendPackMsg(p, MsgFlags.Vital | MsgFlags.NoSend, -1);

                for (var i = 0; i < Players.Length; i++)
                {
                    if (Players[i] != null && Players[i].Team == Players[chatterClientId].Team)
                        Server.SendPackMsg(p, MsgFlags.Vital | MsgFlags.NoRecord, i);
                }
            }
            else
            {
                Server.SendPackMsg(new GameMsg_SvChat
                {
                    ClientId = chatterClientId,
                    Message = msg,
                    IsTeam = false
                }, MsgFlags.Vital, -1);
            }
        }

        public override void OnTick()
        {
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

        public override void OnMessage(int msgId, UnPacker unPacker, int clientId)
        {
            if (!GameMsgUnpacker.UnpackMessage(msgId, unPacker, out var msg, out var error))
            {
                Console.Print(OutputLevel.DEBUG, "server", $"dropped gamemessage='{(GameMessage) msgId}' ({msgId}), failed on '{error}'");
                return;
            }

            var player = Players[clientId];
            
            if (!Server.ClientInGame(clientId))
            {
                if (msg.Type == GameMessage.CL_STARTINFO) 
                    OnMsgStartInfo(player, (GameMsg_ClStartInfo) msg);
            }
            else
            {
                switch (msg.Type)
                {
                    case GameMessage.CL_SAY:
                        OnMsgSay(player, (GameMsg_ClSay)msg);
                        break;

                    case GameMessage.CL_SETTEAM:
                        OnMsgSetTeam(player, (GameMsg_ClSetTeam) msg);
                        break;

                    case GameMessage.CL_SETSPECTATORMODE:
                        OnMsgSetSpectatorMode(player, (GameMsg_ClSetSpectatorMode) msg);
                        break;

                    case GameMessage.CL_CHANGEINFO:
                        break;
                    case GameMessage.CL_KILL:
                        break;
                    case GameMessage.CL_EMOTICON:
                        break;
                    case GameMessage.CL_VOTE:
                        break;
                    case GameMessage.CL_CALLVOTE:
                        break;
                    case GameMessage.CL_ISDDNET:
                        OnMsgIsDDNet(unPacker, player, (GameMsg_ClIsDDNet) msg);
                        break;
                }
            }
        }

        protected virtual void OnMsgSetSpectatorMode(BasePlayer player,
            GameMsg_ClSetSpectatorMode msg)
        {
            if (player.Team != Team.Spectators ||
                player.SpectatorId == msg.SpectatorId ||
                player.ClientId == msg.SpectatorId ||
                Config["SvSpamprotection"] && 
                    player.LastSetSpectatorMode + Server.TickSpeed * 3 > Server.Tick)
            {
                return;
            }

            player.LastSetSpectatorMode = Server.Tick;
            if (msg.SpectatorId != -1 &&
                (msg.SpectatorId < -1 || msg.SpectatorId >= Players.Length ||
                 Players[msg.SpectatorId] == null ||
                 Players[msg.SpectatorId].Team == Team.Spectators))
            {
                SendChatTarget(player.ClientId, "Invalid spectator id");
                return;
            }

            player.SpectatorId = msg.SpectatorId;
        }

        protected virtual void OnMsgSetTeam(BasePlayer player, GameMsg_ClSetTeam msg)
        {
            if (msg.Team < Team.Spectators || msg.Team > Team.Blue) 
                return;

            if (World.IsPaused || player.Team == msg.Team || 
                Config["SvSpamprotection"] &&
                player.LastSetTeam + Server.TickSpeed * 3 > Server.Tick)
            {
                return;
            }

            if (msg.Team != Team.Spectators && LockTeams)
            {
                player.LastSetTeam = Server.Tick;
                SendBroadcast(player.ClientId, "Teams are locked");
                return;
            }

            if (player.TeamChangeTick > Server.Tick)
            {
                player.LastSetTeam = Server.Tick;
                var timeLeft = (player.TeamChangeTick - Server.Tick) / Server.TickSpeed;
                SendBroadcast(player.ClientId, $"Time to wait before changing team: {timeLeft/60}:{timeLeft%60}");
                return;
            }

            if (GameController.CanJoinTeam(player.ClientId, msg.Team))
            {
                if (!GameController.CanChangeTeam(player, msg.Team))
                    return;

                player.LastSetTeam = Server.Tick;
                player.TeamChangeTick = Server.Tick;
                    
                player.SetTeam(msg.Team);
                GameController.CheckTeamBalance();

                if (player.Team == Team.Spectators || msg.Team == Team.Spectators)
                {
                    // vote update
                }
            }
            else
            {
                SendBroadcast(player.ClientId, $"Only {Players.Length - Config["SvSpectatorSlots"]}active players are allowed");
            }
        }

        protected virtual void OnMsgIsDDNet(UnPacker unPacker, BasePlayer player, GameMsg_ClIsDDNet msg)
        {
            var version = unPacker.GetInt();
            if (unPacker.Error)
            {
                if (player.ClientVersion < ClientVersion.DDRACE)
                    player.ClientVersion = ClientVersion.DDRACE;
            }
            else player.ClientVersion = (ClientVersion) version;

            Debug.Warning("ddnet", $"{player.ClientId} using ddnet client ({player.ClientVersion})");
        }

        protected virtual void OnMsgSay(BasePlayer player, GameMsg_ClSay msg)
        {
            if (string.IsNullOrEmpty(msg.Message) ||
                Config["SvSpamprotection"] && player.LastChatMessage + Server.TickSpeed > Server.Tick)
            {
                return;
            }

            msg.Message = msg.Message.Limit(128);
            player.LastChatMessage = Server.Tick;
            SendChat(player.ClientId, msg.IsTeam, msg.Message);
        }

        protected virtual void OnMsgStartInfo(BasePlayer player, 
            GameMsg_ClStartInfo msg)
        {
            if (player.IsReady)
                return;

            player.IsReady = true;
            player.LastChangeInfo = Server.Tick;

            Server.SetClientName(player.ClientId, msg.Name);
            Server.SetClientClan(player.ClientId, msg.Clan);
            Server.SetClientCountry(player.ClientId, msg.Country);

            player.TeeInfo.SkinName = msg.Skin;
            player.TeeInfo.UseCustomColor = msg.UseCustomColor;
            player.TeeInfo.ColorBody = msg.ColorBody;
            player.TeeInfo.ColorFeet = msg.ColorFeet;

            GameController.OnPlayerInfoChange(player);

            // send all votes

            SendTuningParams(player.ClientId);
            Server.SendPackMsg(new GameMsg_SvReadyToEnter(),
                MsgFlags.Vital | MsgFlags.Flush, player.ClientId);
        }

        public override void OnBeforeSnapshots()
        {
        }

        public override void OnAfterSnapshots()
        {
            Events.Clear();
        }

        public override void OnSnapshot(int snappingId)
        {
            World.OnSnapshot(snappingId);
            GameController.OnSnapshot(snappingId);
            Events.OnSnapshot(snappingId);

            for (var i = 0; i < Players.Length; i++)
            {
                if (Players[i] == null)
                    continue;
                Players[i].OnSnapshot(snappingId);
            }

            Players[snappingId].FakeSnapshot(snappingId);
        }

        public override void OnClientConnected(int clientId)
        {
            var startTeam = Config["SvTournamentMode"]
                ? Team.Spectators
                : GameController.GetAutoTeam(clientId);

            Players[clientId] = Kernel.Get<BasePlayer>();
            Players[clientId].Init(clientId, startTeam);

            GameController.CheckTeamBalance();
            GameController.OnClientConnected(clientId);
            // send active vote

            Server.SendPackMsg(new GameMsg_SvMotd {Message = Config["SvMotd"]},
                MsgFlags.Vital | MsgFlags.Flush, clientId);
        }

        public override void OnClientEnter(int clientId)
        {
            Players[clientId].Respawn();

            SendChat(-1, false, $"'{Players[clientId].Name}' entered and joined the {GameController.GetTeamName(Players[clientId].Team)}");
            Console.Print(OutputLevel.DEBUG, "game", $"team_join player='{clientId}:{Players[clientId].Name}' team={Players[clientId].Team}");
            GameController.OnClientEnter(clientId);
            // update vote
        }

        public override void OnClientDrop(int clientId, string reason)
        {
            Players[clientId].OnDisconnect(reason);
            Players[clientId] = null;

            GameController.CheckTeamBalance();
            // update vote

            for (var i = 0; i < Players.Length; i++)
            {
                if (Players[i] == null ||
                    Players[i].SpectatorId != clientId)
                {
                    continue;
                }

                Players[i].SpectatorId = -1;
            }
        }

        public override void OnClientPredictedInput(int clientId, SnapObj_PlayerInput input)
        {
            if (!World.IsPaused)
                Players[clientId].OnPredictedInput(input);
        }

        public override void OnClientDirectInput(int clientId, SnapObj_PlayerInput input)
        {
            if (!World.IsPaused)
                Players[clientId].OnDirectInput(input);
        }
    }
}