using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Server.Game.Entities;
using Math = System.Math;

namespace TeeSharp.Server.Game
{
    public class Player : BasePlayer
    {
        public override void Init(int clientId, bool dummy)
        {
            Server = Kernel.Get<BaseServer>();
            GameContext = Kernel.Get<BaseGameContext>();
            Config = Kernel.Get<BaseConfig>();

            RespawnTick = Server.Tick;
            DieTick = Server.Tick;

            Character = null;
            Team = GameContext.GameController.StartTeam();
            SpectatorMode = SpectatorMode.FreeView;
            SpectatorId = -1;
            SpectatorFlag = null;
            ActiveSpectatorSwitch = false;

            LastActionTick = Server.Tick;
            TeamChangeTick = Server.Tick;
            InactivityTickCounter = 0;
            IsReadyToPlay = !GameContext.GameController.IsPlayerReadyMode();
            RespawnDisabled = GameContext.GameController.GetRespawnDisabled(this);
            DeadSpectatorMode = false;
            Spawning = false;

            ActualLatency = new int[GameContext.Players.Length];
            Latency = new Latency();
            LatestActivity = new Activity();
            PlayerFlags = PlayerFlags.None;
            TeeInfo = new TeeInfo();
        }

        public override void Tick()
        {
            if (!IsDummy && !Server.ClientInGame(ClientId))
                return;


            var info = Server.ClientInfo(ClientId);
            Latency.Accumulate += info.Latency;
            Latency.AccumulateMax = Math.Max(Latency.AccumulateMax, info.Latency);
            Latency.AccumulateMin = Math.Min(Latency.AccumulateMin, info.Latency);

            if (Server.Tick % Server.TickSpeed == 0)
            {
                Latency.Average = Latency.Accumulate / Server.TickSpeed;
                Latency.Max = Latency.AccumulateMax;
                Latency.Min = Latency.AccumulateMin;
                Latency.Accumulate = 0;
                Latency.AccumulateMin = 1000;
                Latency.AccumulateMax = 0;
            }

            if (Character != null && !Character.IsAlive)
                Character = null;

            if (GameContext.GameController.GamePaused)
            {
                RespawnTick++;
                DieTick++;
                LastActionTick++;
                TeamChangeTick++;
            }
            else
            {
                if (Character == null)
                {
                    if (Team == Team.Spectators && SpectatorMode == SpectatorMode.FreeView)
                    {
                        ViewPos -= new Vector2(
                            Math.Clamp(ViewPos.x - LatestActivity.TargetX, -500f, 500f),
                            Math.Clamp(ViewPos.y - LatestActivity.TargetY, -400f, 400f)
                        );
                    }

                    if (DieTick + Server.TickSpeed * 3 <= Server.Tick && !DeadSpectatorMode)
                        Respawn();

                    if (Team == Team.Spectators && SpectatorFlag != null)
                        SpectatorId = SpectatorFlag.Carrier?.Player.ClientId ?? -1;

                    if (Spawning && RespawnTick <= Server.Tick)
                        TryRespawn();
                }
                else if (Character.IsAlive)
                {
                    ViewPos = Character.Position;
                }

                if (!DeadSpectatorMode && LastActionTick != Server.Tick)
                    InactivityTickCounter++;
            }
        }

        public override void PostTick()
        {
            if (PlayerFlags.HasFlag(PlayerFlags.Scoreboard))
            {
                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] != null &&
                        GameContext.Players[i].Team != Team.Spectators)
                    {
                        ActualLatency[i] = GameContext.Players[i].Latency.Min;
                    }
                }
            }

            if ((Team == Team.Spectators || DeadSpectatorMode) && SpectatorMode != SpectatorMode.FreeView)
            {
                if (SpectatorFlag != null)
                    ViewPos = SpectatorFlag.Position;
                else if (GameContext.Players[SpectatorId] != null)
                    ViewPos = GameContext.Players[SpectatorId].ViewPos;
            }
        }

        public override void OnSnapshot(int snappingClient, 
            out SnapshotPlayerInfo playerInfo, 
            out SnapshotSpectatorInfo spectatorInfo,
            out SnapshotDemoClientInfo demoClientInfo)
        {
            playerInfo = null;
            spectatorInfo = null;
            demoClientInfo = null;

            if (!IsDummy && !Server.ClientInGame(ClientId))
                return;

            playerInfo = Server.SnapshotItem<SnapshotPlayerInfo>(ClientId);
            if (playerInfo == null)
                return;

            playerInfo.PlayerFlags = PlayerFlags & PlayerFlags.Chatting;

            if (Server.IsAuthed(ClientId))
                playerInfo.PlayerFlags |= PlayerFlags.Admin;
            if (!GameContext.GameController.IsPlayerReadyMode() || IsReadyToPlay)
                playerInfo.PlayerFlags |= PlayerFlags.Ready;
            if (RespawnDisabled && (Character == null || !Character.IsAlive))
                playerInfo.PlayerFlags |= PlayerFlags.Dead;
            if (snappingClient != -1 && (Team == Team.Spectators || DeadSpectatorMode) && snappingClient == SpectatorId)
                playerInfo.PlayerFlags |= PlayerFlags.Watching;

            playerInfo.Latency = snappingClient == -1
                ? Latency.Min
                : GameContext.Players[snappingClient].ActualLatency[ClientId];
            playerInfo.Score = GameContext.GameController.Score(ClientId);

            if (ClientId == snappingClient && (Team == Team.Spectators || DeadSpectatorMode))
            {
                spectatorInfo = Server.SnapshotItem<SnapshotSpectatorInfo>(ClientId);
                if (spectatorInfo == null)
                    return;

                spectatorInfo.SpectatorMode = SpectatorMode;
                spectatorInfo.SpectatorId = SpectatorId;

                if (SpectatorFlag != null)
                {
                    spectatorInfo.X = (int) SpectatorFlag.Position.x;
                    spectatorInfo.Y = (int) SpectatorFlag.Position.y;
                }
                else
                {
                    spectatorInfo.X = (int) ViewPos.x;
                    spectatorInfo.Y = (int) ViewPos.y;
                }
            }

            if (snappingClient == -1)
            {
                demoClientInfo = Server.SnapshotItem<SnapshotDemoClientInfo>(ClientId);
                if (demoClientInfo == null)
                    return;

                demoClientInfo.Local = 0;
                demoClientInfo.Team = Team;
                demoClientInfo.Name = Server.ClientName(ClientId);
                demoClientInfo.Clan = Server.ClientClan(ClientId);
                demoClientInfo.Country = Server.ClientCountry(ClientId);

                for (var part = SkinPart.Body; part < SkinPart.NumParts; part++)
                    demoClientInfo[part] = TeeInfo[part];
            }
        }

        public override void OnChangeInfo()
        {
            LastChangeInfo = Server.Tick;
        }

        public override void OnDisconnect(string reason)
        {
            KillCharacter(WeaponGame);

            if (Team != Team.Spectators)
            {
                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] != null &&
                        GameContext.Players[i].SpectatorMode == SpectatorMode.Player &&
                        GameContext.Players[i].SpectatorId == ClientId)
                    {
                        if (GameContext.Players[i].DeadSpectatorMode)
                            GameContext.Players[i].UpdateDeadSpecMode();
                        else
                        {
                            GameContext.Players[i].SpectatorMode = SpectatorMode.FreeView;
                            GameContext.Players[i].SpectatorId = -1;
                        }
                    }
                }
            }
        }

        public override void OnSetTeam()
        {
            LastSetTeam = Server.Tick;
        }

        public override void OnChat()
        {
            LastChat = Server.Tick;
        }

        public override void OnPredictedInput(SnapshotPlayerInput input)
        {
            // ignore input when player chat open
            if (PlayerFlags.HasFlag(PlayerFlags.Chatting) && input.PlayerFlags.HasFlag(PlayerFlags.Chatting))
                return;

            Character?.OnPredictedInput(input);
        }

        public override void OnDirectInput(SnapshotPlayerInput input)
        {
            if (GameContext.World.Paused)
            {
                PlayerFlags = input.PlayerFlags;
                return;
            }

            if (input.PlayerFlags.HasFlag(PlayerFlags.Chatting))
            {
                if (PlayerFlags.HasFlag(PlayerFlags.Chatting))
                    return;

                Character?.ResetInput();
                PlayerFlags = input.PlayerFlags;
                return;
            }

            PlayerFlags = input.PlayerFlags;
            Character?.OnDirectInput(input);

            if (Character == null && Team != Team.Spectators && (input.Fire & 1) != 0)
                Respawn();

            if (Character == null && Team == Team.Spectators && (input.Fire & 1) != 0)
            {
                if (!ActiveSpectatorSwitch)
                {
                    ActiveSpectatorSwitch = true;
                    if (SpectatorMode == SpectatorMode.FreeView)
                    {
                        var character = GameContext.World.ClosestEntity<Character>(ViewPos, 6 * 32f, null);
                        var flag = GameContext.World.ClosestEntity<Flag>(ViewPos, 6 * 32f, null);

                        if (character != null || flag != null)
                        {
                            if (character == null || flag != null && character != null &&
                                MathHelper.Distance(ViewPos, flag.Position) <
                                MathHelper.Distance(ViewPos, character.Position))
                            {
                                SpectatorMode = flag.Team == Team.Red
                                    ? SpectatorMode.FlagRed
                                    : SpectatorMode.FlagBlue;
                                SpectatorFlag = flag;
                                SpectatorId = -1;
                            }
                            else
                            {
                                SpectatorMode = SpectatorMode.Player;
                                SpectatorFlag = null;
                                SpectatorId = character.Player.ClientId;
                            }
                        }
                    }
                    else
                    {
                        SpectatorMode = SpectatorMode.FreeView;
                        SpectatorFlag = null;
                        SpectatorId = -1;
                    }
                }
            }
            else if (ActiveSpectatorSwitch)
                ActiveSpectatorSwitch = false;

            if (input.Direction != 0 || 
                LatestActivity.TargetX != input.TargetX ||
                LatestActivity.TargetY != input.TargetY ||
                input.IsJump || (input.Fire & 1) != 0 || input.IsHook)
            {
                LatestActivity.TargetX = input.TargetX;
                LatestActivity.TargetY = input.TargetY;
                LastActionTick = Server.Tick;
                InactivityTickCounter = 0;
            }
        }

        public override Character GetCharacter()
        {
            return Character != null && Character.IsAlive ? Character : null;
        }

        public override void KillCharacter(Weapon weapon)
        {
            if (Character == null)
                return;

            Character.Die(ClientId, weapon);
            Character = null;
        }

        public override void Respawn()
        {
            if (RespawnDisabled && Team != Team.Spectators)
            {
                DeadSpectatorMode = true;
                IsReadyToPlay = true;
                SpectatorMode = SpectatorMode.Player;
                UpdateDeadSpecMode();
                return;
            }

            DeadSpectatorMode = false;

            if (Team != Team.Spectators)
                Spawning = true;
        }

        public override bool SetSpectatorID(SpectatorMode mode, int spectatorId)
        {
            if (SpectatorMode == mode && mode != SpectatorMode.Player ||
                SpectatorMode == SpectatorMode.Player && mode == SpectatorMode.Player &&
                (spectatorId == -1 || SpectatorId == spectatorId || ClientId == spectatorId))
            {
                return false;
            }

            if (Team == Team.Spectators)
            {
                if (mode != SpectatorMode.Player ||
                    mode == SpectatorMode.Player &&
                    GameContext.Players[SpectatorId] != null &&
                    GameContext.Players[SpectatorId].Team != Team.Spectators)
                {
                    if (mode == SpectatorMode.FlagRed || mode == SpectatorMode.FlagBlue)
                    {
                        foreach (var flag in GameContext.World.GetEntities<Flag>())
                        {
                            if (flag.Team == Team.Red && mode == SpectatorMode.FlagRed ||
                                flag.Team == Team.Blue && mode == SpectatorMode.FlagBlue)
                            {
                                SpectatorFlag = null;
                                SpectatorId = flag.Carrier?.Player.ClientId ?? -1;
                                break;
                            }
                        }

                        if (SpectatorFlag == null)
                            return false;

                        SpectatorMode = mode;
                        return true;
                    }

                    SpectatorFlag = null;
                    SpectatorMode = mode;
                    SpectatorId = spectatorId;
                    return true;
                }
            } 
            else if (DeadSpectatorMode)
            {
                if (mode == SpectatorMode.Player && DeadCanFollow(GameContext.Players[SpectatorId]))
                {
                    SpectatorMode = mode;
                    SpectatorFlag = null;
                    SpectatorId = spectatorId;
                    return true;
                }
            }

            return false;
        }

        public override bool DeadCanFollow(BasePlayer player)
        {
            if (player == null)
                return false;

            return (!player.RespawnDisabled ||
                     player.GetCharacter() != null &&
                     player.GetCharacter().IsAlive) && player.Team == Team;
        }

        public override void UpdateDeadSpecMode()
        {
            if (SpectatorId != -1 && DeadCanFollow(GameContext.Players[SpectatorId]))
                return;

            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null)
                    continue;

                if (DeadCanFollow(GameContext.Players[i]))
                {
                    SpectatorId = i;
                    return;
                }
            }

            DeadSpectatorMode = false;
        }

        public override void ReadyToEnter()
        {
            IsReadyToEnter = true;
            Server.SendPackMsg(new GameMsg_SvReadyToEnter(), MsgFlags.Vital | MsgFlags.Flush, ClientId);
        }

        public override void SetTeam(Team team)
        {
            if (Team == team)
                return;

            var prevTeam = Team;

            KillCharacter(WeaponGame);

            Team = team;
            LastActionTick = Server.Tick;
            TeamChangeTick = Server.Tick;
            SpectatorMode = SpectatorMode.FreeView;
            SpectatorId = -1;
            SpectatorFlag = null;
            DeadSpectatorMode = false;
            RespawnTick = Server.Tick + Server.TickSpeed / 2;

            Server.SendPackMsg(new GameMsg_SvTeam()
            {
                ClientId = ClientId,
                Team = team,
                Silent = true, // TODO
                CooldownTick = TeamChangeTick,
            }, MsgFlags.Vital, -1);

            if (prevTeam == Team.Spectators)
                InactivityTickCounter = 0;

            if (Team == Team.Spectators)
            {
                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] == null)
                        continue;

                    if (GameContext.Players[i].SpectatorMode == SpectatorMode.Player &&
                        GameContext.Players[i].SpectatorId == ClientId)
                    {
                        if (GameContext.Players[i].DeadSpectatorMode)
                            GameContext.Players[i].UpdateDeadSpecMode();
                        else
                        {
                            GameContext.Players[i].SpectatorMode = SpectatorMode.FreeView;
                            GameContext.Players[i].SpectatorId = -1;
                        }
                    }
                }
            }
        }

        protected override void TryRespawn()
        {
            if (!GameContext.GameController.CanSpawn(Team, ClientId, out var spawnPos))
                return;

            Spawning = false;
            Character = new Character(this, spawnPos);
        }
    }
}