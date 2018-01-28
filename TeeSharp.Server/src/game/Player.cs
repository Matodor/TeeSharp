using System;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

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
            IsReady = false;
            LastChangeInfo = -1;
            SpectatorId = -1;
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

            }
            else
            {
                
            }
        }

        public override void PostTick()
        {
            if (PlayerFlags.HasFlag(PlayerFlags.PLAYERFLAG_SCOREBOARD))
            {
                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] == null ||
                        GameContext.Players[i].Team == Team.SPECTATORS)
                    {
                        continue;
                    }

                    ActLatency[i] = GameContext.Players[i].Latency.Min;
                }
            }

            if (SpectatorId >= 0 && Team == Team.SPECTATORS && GameContext.Players[SpectatorId] != null)
                ViewPos = GameContext.Players[SpectatorId].ViewPos;
        }

        public override void Respawn()
        {
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

            clientInfo.Name = Server.GetClientName(ClientId);
            clientInfo.Clan = Server.GetClientClan(ClientId);
            clientInfo.Country = Server.GetClientCountry(ClientId);
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
        }

        public override void OnDirectInput(SnapObj_PlayerInput input)
        {
        }
    }
}