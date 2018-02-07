using System;
using TeeSharp.Common;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Server.Game.Entities;
using Math = System.Math;

namespace TeeSharp.Server.Game
{
    public class Player : BasePlayer
    {
        public override void Init(int clientId, Team startTeam)
        {
            base.Init(clientId, startTeam);

            Team = startTeam;
            ActLatency = new int[Server.MaxClients];
            Latency = new Latency();
            TeeInfo = new TeeInfo();
            LatestActivity = new Activity();
            IsReady = false;
            LastSetTeam = Server.Tick;
            LastChangeInfo = -1;
            SpectatorId = -1;
            Spawning = false;

            var idMap = BaseServer.GetIdMap(clientId);
            for (var i = 1; i < BaseServer.VANILLA_MAX_CLIENTS; i++)
                Server.IdMap[idMap + i] = -1;
            Server.IdMap[idMap] = clientId;
        }

        public override Character GetCharacter()
        {
            if (Character != null && Character.IsAlive)
                return Character;
            return null;
        }

        public override void Tick()
        {
            if (!Server.ClientInGame(ClientId))
                return;

            if (Server.GetClientInfo(ClientId, out var info))
            {
                Latency.Accumulate += info.Latency;
                Latency.AccumulateMax = Math.Max(Latency.AccumulateMax, info.Latency);
                Latency.AccumulateMin = Math.Min(Latency.AccumulateMin, info.Latency);
            }

            if (Server.Tick % Server.TickSpeed == 0)
            {
                Latency.Average = Latency.Accumulate / Server.TickSpeed;
                Latency.Max = Latency.AccumulateMax;
                Latency.Min = Latency.AccumulateMin;
                Latency.Accumulate = 0;
                Latency.AccumulateMax = 0;
                Latency.AccumulateMin = 1000;
            }

            if (GameContext.World.IsPaused)
            {
                RespawnTick++;
                LastActionTick++;
                TeamChangeTick++;
                DieTick++;
            }
            else
            {
                if (Character == null && Team == Team.SPECTATORS && SpectatorId == -1)
                {
                    ViewPos -= new Vec2(
                        Math.Clamp(ViewPos.x - LatestActivity.TargetX, -500f, 500f),
                        Math.Clamp(ViewPos.y - LatestActivity.TargetY, -400f, 400f)
                    );
                }

                if (Character == null && DieTick + Server.TickSpeed * 3 <= Server.Tick)
                    Spawning = true;

                if (Character != null)
                {
                    if (Character.IsAlive)
                        ViewPos = Character.Position;
                    else
                        Character = null;
                }
                else if (Spawning && RespawnTick <= Server.Tick)
                    TryRespawn();
            }
        }

        public override void PostTick()
        {
            if (PlayerFlags.HasFlag(PlayerFlags.PLAYERFLAG_SCOREBOARD))
            {
                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] != null &&
                        GameContext.Players[i].Team != Team.SPECTATORS)
                    {
                        ActLatency[i] = GameContext.Players[i].Latency.Min;
                    }
                }
            }

            if (SpectatorId >= 0 && Team == Team.SPECTATORS && GameContext.Players[SpectatorId] != null)
                ViewPos = GameContext.Players[SpectatorId].ViewPos;
        }

        public override void SetTeam(Team team)
        {
            team = GameContext.GameController.ClampTeam(team);
            if (team == Team)
                return;

            GameContext.SendChat(-1, false, $"'{Name}' joined the {GameContext.GameController.GetTeamName(team)}");
            KillCharacter();

            Team = team;
            LastActionTick = Server.Tick;
            SpectatorId = -1;
            RespawnTick = Server.Tick + Server.TickSpeed / 2;
            GameContext.Console.Print(OutputLevel.DEBUG, "game",
                $"team_join player='{ClientId}:{Name}' team={Team}");
            GameContext.GameController.OnPlayerInfoChange(this);

            if (Team == Team.SPECTATORS)
            {
                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] == null ||
                        GameContext.Players[i].SpectatorId != ClientId)
                    {
                        continue;
                    }

                    GameContext.Players[i].SpectatorId = -1;
                }
            }
        }

        public override void KillCharacter(Weapon weapon = Weapon.GAME)
        {
            if (Character == null)
                return;

            Character.Die(ClientId, weapon);
            Character = null;
        }

        public override void Respawn()
        {
            if (Team != Team.SPECTATORS)
                Spawning = true;
        }

        public override void OnDisconnect(string reason)
        {
            KillCharacter();

            if (!Server.ClientInGame(ClientId))
                return;

            GameContext.SendChat(-1, false,
                string.IsNullOrWhiteSpace(reason)
                    ? $"'{Name}' has left the game"
                    : $"'{Name}' has left the game ({reason})");

            GameContext.Console.Print(OutputLevel.STANDARD, "game", $"leave_player='{ClientId}:{Name}'");
        }

        public override void OnSnapshot(int snappingClient)
        {
            if (!Server.ClientInGame(ClientId))
                return;

            var id = ClientId;
            if (!Server.Translate(ref id, snappingClient))
                return;

            var clientInfo = Server.SnapObject<SnapObj_ClientInfo>(id);
            if (clientInfo == null)
                return;

            clientInfo.Name = Name;
            clientInfo.Clan = Clan;
            clientInfo.Country = Country;
            clientInfo.Skin = TeeInfo.SkinName;
            clientInfo.UseCustomColor = TeeInfo.UseCustomColor;
            clientInfo.ColorBody = TeeInfo.ColorBody;
            clientInfo.ColorFeet = TeeInfo.ColorFeet;

            var playerInfo = Server.SnapObject<SnapObj_PlayerInfo>(id);
            if (playerInfo == null)
                return;

            playerInfo.Team = Team;
            playerInfo.Score = Server.GetClientScore(ClientId);
            playerInfo.ClientId = id;
            playerInfo.Local = ClientId == snappingClient ? 1 : 0;
            playerInfo.Latency = snappingClient == -1
                ? Latency.Min
                : GameContext.Players[snappingClient].ActLatency[ClientId];

            if (ClientId == snappingClient && Team == Team.SPECTATORS)
            {
                var spectatorInfo = Server.SnapObject<SnapObj_SpectatorInfo>(ClientId);
                if (spectatorInfo == null)
                    return;

                spectatorInfo.SpectatorId = SpectatorId;
                spectatorInfo.ViewPos = ViewPos;
            }
        }

        public override void FakeSnapshot(int snappingClient)
        {
            if (!Server.GetClientInfo(snappingClient, out var info))
                return;

            if (GameContext.Players[snappingClient] != null &&
                GameContext.Players[snappingClient].ClientVersion >= ClientVersion.DDNET_OLD)
            {
                return;
            }

            var id = BaseServer.VANILLA_MAX_CLIENTS - 1;
            var clientInfo = Server.SnapObject<SnapObj_ClientInfo>(id);

            if (clientInfo == null)
                return;

            clientInfo.Name = " ";
            clientInfo.Clan = Server.GetClientClan(ClientId);
            clientInfo.Skin = TeeInfo.SkinName;
        }

        public override void OnPredictedInput(SnapObj_PlayerInput input)
        {
            // ignore input when player chat open
            if (PlayerFlags.HasFlag(PlayerFlags.PLAYERFLAG_CHATTING) &&
                input.PlayerFlags.HasFlag(PlayerFlags.PLAYERFLAG_CHATTING))
            {
                return;
            }

            Character?.OnPredictedInput(input);
        }

        public override void OnDirectInput(SnapObj_PlayerInput input)
        {
            if (input.PlayerFlags.HasFlag(PlayerFlags.PLAYERFLAG_CHATTING))
            {
                if (PlayerFlags.HasFlag(PlayerFlags.PLAYERFLAG_CHATTING))
                    return;

                Character?.ResetInput();
                PlayerFlags = input.PlayerFlags;
                return;
            }

            PlayerFlags = input.PlayerFlags;
            Character?.OnDirectInput(input);

            if (Character == null && Team != Team.SPECTATORS && (input.Fire & 1) != 0)
                Spawning = true;

            if (input.Direction != 0 ||
                input.Jump ||
                input.Hook ||
                LatestActivity.TargetX != input.TargetX ||
                LatestActivity.TargetY != input.TargetY ||
                (input.Fire & 1) != 0)
            {
                LatestActivity.TargetX = input.TargetX;
                LatestActivity.TargetY = input.TargetY;
                LastActionTick = Server.Tick;
            }
        }

        protected override void TryRespawn()
        {
            if (!GameContext.GameController.CanSpawn(Team, ClientId, out var spawnPos))
                return;

            Spawning = false;
            Character = new Character(this, spawnPos);
            GameContext.CreatePlayerSpawn(spawnPos);
        }
    }
}