using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    public class CGameControllerCTF : IGameController
    {
        private CFlag[] m_apFlags;

        public CGameControllerCTF(CGameContext pGameServer) : base(pGameServer)
        {
            m_apFlags = new CFlag[] {null, null};
            m_pGameType = "CTF";
            m_GameFlags = (int)Consts.GAMEFLAG_TEAMS | (int)Consts.GAMEFLAG_FLAGS;
        }

        public override bool OnEntity(MapItems tile, vec2 Pos)
        {
            if (base.OnEntity(tile, Pos))
                return true;

            int Team = -1;
            if (tile == MapItems.ENTITY_FLAGSTAND_RED) Team = (int)Consts.TEAM_RED;
            if (tile == MapItems.ENTITY_FLAGSTAND_BLUE) Team = (int)Consts.TEAM_BLUE;
            if (Team == -1 || m_apFlags[Team] != null)
                return false;

            CFlag F = new CFlag(GameServer.m_World, Team);
            F.m_StandPos = Pos;
            F.m_Pos = Pos;
            m_apFlags[Team] = F;
            GameServer.m_World.InsertEntity(F);
            return true;
        }

        public override int OnCharacterDeath(CCharacter pVictim, CPlayer pKiller, int WeaponID)
        {
            base.OnCharacterDeath(pVictim, pKiller, WeaponID);
	        int HadFlag = 0;

	        // drop flags
	        for(int i = 0; i< 2; i++)
	        {
		        CFlag F = m_apFlags[i];
		        if(F != null && pKiller?.GetCharacter() != null && F.m_pCarryingCharacter == pKiller.GetCharacter())
			        HadFlag |= 2;
		        if(F != null && F.m_pCarryingCharacter == pVictim)
		        {
                    GameServer.CreateSoundGlobal((int)Consts.SOUND_CTF_DROP);
                F.m_DropTick = Server.Tick();
                F.m_pCarryingCharacter = null;
			        F.m_Vel = new vec2(0,0);

			        if(pKiller != null && pKiller.GetTeam() != pVictim.GetPlayer().GetTeam())
				        pKiller.m_Score++;

			        HadFlag |= 1;
		        }
        }

	        return HadFlag;
        }

        public override void DoWincheck()
        {
            if (m_GameOverTick == -1 && m_Warmup == 0)
            {
                // check score win condition
                if ((g_Config.GetInt("SvScorelimit") > 0 && (m_aTeamscore[(int)Consts.TEAM_RED] >= g_Config.GetInt("SvScorelimit") || m_aTeamscore[(int)Consts.TEAM_BLUE] >= g_Config.GetInt("SvScorelimit"))) ||
                    (g_Config.GetInt("SvTimelimit") > 0 && (Server.Tick() - m_RoundStartTick) >= g_Config.GetInt("SvTimelimit") * Server.TickSpeed() * 60))
                {
                    if (m_SuddenDeath != 0)
                    {
                        if (m_aTeamscore[(int)Consts.TEAM_RED] / 100 != m_aTeamscore[(int)Consts.TEAM_BLUE] / 100)
                            EndRound();
                    }
                    else
                    {
                        if (m_aTeamscore[(int)Consts.TEAM_RED] != m_aTeamscore[(int)Consts.TEAM_BLUE])
                            EndRound();
                        else
                            m_SuddenDeath = 1;
                    }
                }
            }
        }

        public override bool CanBeMovedOnBalance(int ClientID)
        {
            CCharacter Character = GameServer.m_apPlayers[ClientID].GetCharacter();
            if (Character != null)
            {
                for (int fi = 0; fi < 2; fi++)
                {
                    CFlag F = m_apFlags[fi];
                    if (F != null && F.m_pCarryingCharacter == Character)
                        return false;
                }
            }
            return true;
        }

        public override void Snap(int SnappingClient)
        {
            base.Snap(SnappingClient);

            CNetObj_GameData pGameDataObj = Server.SnapNetObj<CNetObj_GameData>((int)Consts.NETOBJTYPE_GAMEDATA, 0);
            if (pGameDataObj == null)
                return;

            pGameDataObj.m_TeamscoreRed = m_aTeamscore[(int)Consts.TEAM_RED];
            pGameDataObj.m_TeamscoreBlue = m_aTeamscore[(int)Consts.TEAM_BLUE];

            if (m_apFlags[(int)Consts.TEAM_RED] != null)
            {
                if (m_apFlags[(int)Consts.TEAM_RED].m_AtStand != 0)
                    pGameDataObj.m_FlagCarrierRed = (int)Consts.FLAG_ATSTAND;
                else if (m_apFlags[(int)Consts.TEAM_RED].m_pCarryingCharacter?.GetPlayer() != null)
                    pGameDataObj.m_FlagCarrierRed = m_apFlags[(int)Consts.TEAM_RED].m_pCarryingCharacter.GetPlayer().GetCID();
                else
                    pGameDataObj.m_FlagCarrierRed = (int)Consts.FLAG_TAKEN;
            }
            else
                pGameDataObj.m_FlagCarrierRed = (int)Consts.FLAG_MISSING;
            if (m_apFlags[(int)Consts.TEAM_BLUE] != null)
            {
                if (m_apFlags[(int)Consts.TEAM_BLUE].m_AtStand != 0)
                    pGameDataObj.m_FlagCarrierBlue = (int)Consts.FLAG_ATSTAND;
                else if (m_apFlags[(int)Consts.TEAM_BLUE].m_pCarryingCharacter?.GetPlayer() != null)
                    pGameDataObj.m_FlagCarrierBlue = m_apFlags[(int)Consts.TEAM_BLUE].m_pCarryingCharacter.GetPlayer().GetCID();
                else
                    pGameDataObj.m_FlagCarrierBlue = (int)Consts.FLAG_TAKEN;
            }
            else
                pGameDataObj.m_FlagCarrierBlue = (int)Consts.FLAG_MISSING;
        }

        public override void Tick()
        {
            base.Tick();

            if (GameServer.m_World.m_ResetRequested || GameServer.m_World.m_Paused)
                return;

            for (int fi = 0; fi < 2; fi++)
            {
                CFlag F = m_apFlags[fi];

                if (F == null)
                    continue;

                // flag hits death-tile or left the game layer, reset it
                if ((GameServer.Collision.GetCollisionAt(F.m_Pos.x, F.m_Pos.y) & CCollision.COLFLAG_DEATH) != 0 || F.GameLayerClipped(F.m_Pos))
                {
                    GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", "flag_return");
                    GameServer.CreateSoundGlobal((int)Consts.SOUND_CTF_RETURN);
                    F.Reset();
                    continue;
                }

                //
                if (F.m_pCarryingCharacter != null)
                {
                    // update flag position
                    F.m_Pos = F.m_pCarryingCharacter.m_Pos;

                    if (m_apFlags[fi ^ 1] != null && m_apFlags[fi ^ 1].m_AtStand != 0)
                    {
                        if (VMath.distance(F.m_Pos, m_apFlags[fi ^ 1].m_Pos) < CFlag.ms_PhysSize + CCharacter.ms_PhysSize)
                        {
                            // CAPTURE! \o/
                            m_aTeamscore[fi ^ 1] += 100;
                            F.m_pCarryingCharacter.GetPlayer().m_Score += 5;

                            string aBuf = string.Format("flag_capture player='{0}:{1}'",
                                F.m_pCarryingCharacter.GetPlayer().GetCID(),
                                Server.ClientName(F.m_pCarryingCharacter.GetPlayer().GetCID()));
                            GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", aBuf);

                            float CaptureTime = (Server.Tick() - F.m_GrabTick) / (float)Server.TickSpeed();
                            if (CaptureTime <= 60)
                            {
                                aBuf = string.Format("The {0} flag was captured by '{1}' ({2}.{3}{4} seconds)", fi != 0 ? "blue" : "red", Server.ClientName(F.m_pCarryingCharacter.GetPlayer().GetCID()), (int)CaptureTime % 60, ((int)(CaptureTime * 100) % 100) < 10 ? "0" : "", (int)(CaptureTime * 100) % 100);
                            }
                            else
                            {
                                aBuf = string.Format("The {0} flag was captured by '{1}'", fi != 0 ? "blue" : "red", Server.ClientName(F.m_pCarryingCharacter.GetPlayer().GetCID()));
                            }
                            GameServer.SendChat(-1, -2, aBuf);
                            for (int i = 0; i < 2; i++)
                                m_apFlags[i].Reset();

                            GameServer.CreateSoundGlobal((int)Consts.SOUND_CTF_CAPTURE);
                        }
                    }
                }
                else
                {
                    CEntity[] apCloseCCharacters = new CEntity[(int)Consts.MAX_CLIENTS];
                    int Num = GameServer.m_World.FindEntities(F.m_Pos, CFlag.ms_PhysSize, ref apCloseCCharacters, (int)Consts.MAX_CLIENTS, CGameWorld.ENTTYPE_CHARACTER);

                    for (int i = 0; i < Num; i++)
                    {
                        vec2 out1, out2;
                        CCharacter character = (CCharacter) apCloseCCharacters[i];
                        if (!character.IsAlive() || character.GetPlayer().GetTeam() == (int)Consts.TEAM_SPECTATORS || GameServer.Collision.IntersectLine(F.m_Pos, character.m_Pos, out out1, out out2) != 0)
                            continue;

                        if (character.GetPlayer().GetTeam() == F.m_Team)
                        {
                            // return the flag
                            if (F.m_AtStand == 0)
                            {
                                character.GetPlayer().m_Score += 1;

                                string aBuf = string.Format("flag_return player='{0}:{1}'",
                                    character.GetPlayer().GetCID(),
                                    Server.ClientName(character.GetPlayer().GetCID()));
                                GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", aBuf);

                                GameServer.CreateSoundGlobal((int)Consts.SOUND_CTF_RETURN);
                                F.Reset();
                            }
                        }
                        else
                        {
                            // take the flag
                            if (F.m_AtStand != 0)
                            {
                                m_aTeamscore[fi ^ 1]++;
                                F.m_GrabTick = Server.Tick();
                            }

                            F.m_AtStand = 0;
                            F.m_pCarryingCharacter = character;
                            F.m_pCarryingCharacter.GetPlayer().m_Score += 1;

                            string aBuf = string.Format("flag_grab player='{0}:{1}'",
                                F.m_pCarryingCharacter.GetPlayer().GetCID(),
                                Server.ClientName(F.m_pCarryingCharacter.GetPlayer().GetCID()));
                            GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", aBuf);

                            for (int c = 0; c < (int)Consts.MAX_CLIENTS; c++)
                            {
                                CPlayer pPlayer = GameServer.m_apPlayers[c];
                                if (pPlayer == null)
                                    continue;

                                if (pPlayer.GetTeam() == (int)Consts.TEAM_SPECTATORS && pPlayer.m_SpectatorID != (int)Consts.SPEC_FREEVIEW && GameServer.m_apPlayers[pPlayer.m_SpectatorID] != null && GameServer.m_apPlayers[pPlayer.m_SpectatorID].GetTeam() == fi)
                                    GameServer.CreateSoundGlobal((int)Consts.SOUND_CTF_GRAB_EN, c);
                                else if (pPlayer.GetTeam() == fi)
                                    GameServer.CreateSoundGlobal((int)Consts.SOUND_CTF_GRAB_EN, c);
                                else
                                    GameServer.CreateSoundGlobal((int)Consts.SOUND_CTF_GRAB_PL, c);
                            }
                            // demo record entry
                            GameServer.CreateSoundGlobal((int)Consts.SOUND_CTF_GRAB_EN, -2);
                            break;
                        }
                    }

                    if (F.m_pCarryingCharacter == null && F.m_AtStand == 0)
                    {
                        if (Server.Tick() > F.m_DropTick + Server.TickSpeed() * 30)
                        {
                            GameServer.CreateSoundGlobal((int)Consts.SOUND_CTF_RETURN);
                            F.Reset();
                        }
                        else
                        {
                            F.m_Vel.y += GameServer.Tuning["Gravity"];
                            GameServer.Collision.MoveBox(ref F.m_Pos, ref F.m_Vel, new vec2(CFlag.ms_PhysSize, CFlag.ms_PhysSize), 0.5f);
                        }
                    }
                }
            }
        }
    }
}
