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
        public override BasePlayer[] Players { get; protected set; }
        public override BaseGameController GameController { get; protected set; }
        protected override BaseConfig Config { get; set; }
        protected override BaseGameConsole Console { get; set; }

        protected override BaseServer Server { get; set; }
        protected override BaseLayers Layers { get; set; }
        protected override BaseCollision Collision { get; set; }
        protected override BaseGameMsgUnpacker GameMsgUnpacker { get; set; }

        public override void RegisterConsoleCommands()
        {
        }

        public override bool IsClientInGame(int clientId)
        {
            return false;
        }

        public override bool IsClientReady(int clientId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnInit()
        {
            Server = Kernel.Get<BaseServer>();
            Layers = Kernel.Get<BaseLayers>();
            GameMsgUnpacker = Kernel.Get<BaseGameMsgUnpacker>();
            Collision = Kernel.Get<BaseCollision>();
            Config = Kernel.Get<BaseConfig>();
            Console = Kernel.Get<BaseGameConsole>();

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

                    if (tile.Index >= MapContainer.ENTITY_OFFSET)
                        GameController.OnEntity(tile.Index - MapContainer.ENTITY_OFFSET, pos);
                }
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
            
        }

        public override void OnBeforeSnapshot()
        {
        }

        public override void OnAfterSnapshot()
        {
        }

        public override void OnSnapshot(int clientId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnClientConnected(int clientId)
        {
            var startTeam = Config["SvTournamentMode"]
                ? Team.SPECTATORS
                : GameController.GetAutoTeam(clientId);

            Players[clientId] = Kernel.Get<BasePlayer>();
            Players[clientId].Init(clientId, startTeam);

            GameController.CheckTeamsBalance();

            // send vote

            Server.SendPackMsg(new GameMsg_SvMotd {Message = Config["SvMotd"]},
                MsgFlags.VITAL | MsgFlags.FLUSH, clientId);
        }

        public override void OnClientEnter(int clientId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnClientDrop(int clientId, string reason)
        {
        }

        public override void OnClientPredictedInput(int clientId, NetObj_PlayerInput input)
        {
            throw new System.NotImplementedException();
        }

        public override void OnClientDirectInput(int clientId, NetObj_PlayerInput input)
        {
            throw new System.NotImplementedException();
        }
    }
}