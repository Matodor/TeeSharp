using System.Linq;
using System.Xml.XPath;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Map;

namespace TeeSharp.Server.Game
{
    public class GameContext : BaseGameContext
    {
        public override string GameVersion { get; } = "0.6";
        public override string NetVersion { get; } = "0.6";
        public override string ReleaseVersion { get; } = "0.63";

        public override void OnInit()
        {
            Server = Kernel.Get<BaseServer>();
            Layers = Kernel.Get<BaseLayers>();
            GameMsgUnpacker = Kernel.Get<BaseGameMsgUnpacker>();
            Collision = Kernel.Get<BaseCollision>();
            Config = Kernel.Get<BaseConfig>();
            Console = Kernel.Get<BaseGameConsole>();
            Tuning = Kernel.Get<BaseTuningParams>();

            Layers.Init(Server.CurrentMap);
            Collision.Init(Layers);
            Players = new BasePlayer[Server.MaxClients];
            
            // TODO
            GameController = new GameControllerDM();
            GameController.Init();

            for (var y = 0; y < Layers.GameLayer.Height; y++)
            {
                for (var x = 0; x < Layers.GameLayer.Width; x++)
                {
                    var tile = Collision.GetTileAtIndex(y * Layers.GameLayer.Width + x);
                    var pos = new Vector2(x * 32.0f + 16.0f, y * 32.0f + 16.0f);

                    if (tile.Index >= MapContainer.ENTITY_OFFSET)
                        GameController.OnEntity(tile.Index - MapContainer.ENTITY_OFFSET, pos);
                }
            }
        }


        public override void RegisterConsoleCommands()
        {
        }

        public override bool IsClientSpectator(int clientId)
        {
            return Players[clientId] != null && Players[clientId].Team == Team.SPECTATORS;
        }

        public override bool IsClientReady(int clientId)
        {
            return Players[clientId] != null && Players[clientId].IsReady;
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
            var msg = new MsgPacker((int) GameMessages.SV_TUNEPARAMS);
            foreach (var pair in Tuning)
                msg.AddInt(pair.Value.Value);
            Server.SendMsg(msg, MsgFlags.VITAL, clientId);
        }

        public override void SendChatTarget(int clientId, string msg)
        {
            Server.SendPackMsg(new GameMsg_SvChat
            {
                IsTeam = false,
                ClientId = -1,
                Message = msg
            }, MsgFlags.VITAL, clientId);
        }

        public override void SendChat(int chatterClientId, bool isTeamChat, string msg)
        {
            string debug;
            if (chatterClientId >= 0 && chatterClientId < Server.MaxClients)
                debug = $"{chatterClientId}:{isTeamChat}:{Server.GetClientName(chatterClientId)} {msg}";
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
                Server.SendPackMsg(p, MsgFlags.VITAL | MsgFlags.NOSEND, -1);

                for (var i = 0; i < Players.Length; i++)
                {
                    if (Players[i] != null && Players[i].Team == Players[chatterClientId].Team)
                        Server.SendPackMsg(p, MsgFlags.VITAL | MsgFlags.NORECORD, i);
                }
            }
            else
            {
                Server.SendPackMsg(new GameMsg_SvChat
                {
                    ClientId = chatterClientId,
                    Message = msg,
                    IsTeam = false
                }, MsgFlags.VITAL, -1);
            }
        }

        public override void OnTick()
        {
        }

        public override void OnShutdown()
        {
            throw new System.NotImplementedException();
        }

        public override void OnMessage(int msgId, Unpacker unpacker, int clientId)
        {
            if (!GameMsgUnpacker.Unpack(msgId, unpacker, out var msg, out var error))
            {
                Console.Print(OutputLevel.DEBUG, "server", $"dropped gamemessage='{(GameMessages) msgId}' ({msgId}), failed on '{error}'");
                return;
            }

            var player = Players[clientId];
            
            if (!Server.ClientInGame(clientId))
            {
                if (msg.MsgId == GameMessages.CL_STARTINFO) 
                    OnMsgStartInfo(player, (GameMsg_ClStartInfo) msg);
            }
            else
            {
                
            }
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
                MsgFlags.VITAL | MsgFlags.FLUSH, player.ClientId);
        }

        public override void OnBeforeSnapshot()
        {
        }

        public override void OnAfterSnapshot()
        {
        }

        public override void OnSnapshot(int clientId)
        {
        }

        public override void OnClientConnected(int clientId)
        {
            var startTeam = Config["SvTournamentMode"]
                ? Team.SPECTATORS
                : GameController.GetAutoTeam(clientId);

            Players[clientId] = Kernel.Get<BasePlayer>();
            Players[clientId].Init(clientId, startTeam);

            GameController.CheckTeamsBalance();

            // send active vote

            Server.SendPackMsg(new GameMsg_SvMotd {Message = Config["SvMotd"]},
                MsgFlags.VITAL | MsgFlags.FLUSH, clientId);
        }

        public override void OnClientEnter(int clientId)
        {
            Players[clientId].Respawn();

            SendChat(-1, false, $"'{Server.GetClientName(clientId)}' entered and joined the {GameController.GetTeamName(Players[clientId].Team)}");
            Console.Print(OutputLevel.DEBUG, "game", $"team_join player='{clientId}:{Server.GetClientName(clientId)}' team={Players[clientId].Team}");
            
            // update vote
        }

        public override void OnClientDrop(int clientId, string reason)
        {
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