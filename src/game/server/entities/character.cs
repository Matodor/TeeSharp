using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    public class CCharacter : CEntity
    {
        public const int
            WEAPON_GAME = -3,  // team switching etc
            WEAPON_SELF = -2,  // console kill command
            WEAPON_WORLD = -1; // death tiles etc

        public const int ms_PhysSize = 28;
        public MapItems PrevTile { get { return _prevTile; } }
        public bool IsAlive() { return _alive; }
        public CPlayer GetPlayer() { return _player; }

        private readonly CCharacterCore _core;
        private CPlayer _player;
        private MapItems _prevTile;

        private vec2 _prevVel;
        private bool _prevIsGrounded;
        private bool _alive;
        private readonly CDataContainer g_pData = CDataContainer.datacontainer;

        // weapon info
        private readonly CEntity[] _apHitObjects;
        private int _numObjectsHit;

        private class WeaponStat
        {
            public int m_AmmoRegenStart;
            public int m_Ammo;
            public int m_Ammocost;
            public bool m_Got;
        }

        private readonly WeaponStat[] _aWeapons;
        private int _activeWeapon;
        private int _lastWeapon;
        private int _queuedWeapon;

        private int _reloadTimer;
        private int _attackTick;

        private int _damageTaken;

        private int _emoteType;
        private int _emoteStop;

        // last tick that the player took any action ie some input
        private int _lastAction;
        private int _lastNoAmmoSound;

        // these are non-heldback inputs
        private readonly CNetObj_PlayerInput _latestPrevInput;
        private readonly CNetObj_PlayerInput _latestInput;

        // input
        private readonly CNetObj_PlayerInput _prevInput;
        private readonly CNetObj_PlayerInput _input;
        private int _numInputs;
        private int _damageTakenTick;
        private int _health;
        private int _armor;

        // ninja
        private class NinjaStats
        {
            public vec2 m_ActivationDir;
            public int m_ActivationTick;
            public int m_CurrentMoveTime;
            public int m_OldVelAmount;
        }
        private readonly NinjaStats _ninja;

        // info for dead reckoning
        private int _reckoningTick; // tick that we are performing dead reckoning From
        private CCharacterCore _sendCore; // core that we should send
        private CCharacterCore _reckoningCore; // the dead reckoning core
        private readonly CConfiguration g_Config;

        public CCharacter(CGameWorld pGameWorld) : base(pGameWorld, CGameWorld.ENTTYPE_CHARACTER)
        {
            m_ProximityRadius = ms_PhysSize;
            _health = 0;
            _armor = 0;

            _apHitObjects = new CEntity[10];
            _latestPrevInput = new CNetObj_PlayerInput();
            _latestInput = new CNetObj_PlayerInput();
            _prevInput = new CNetObj_PlayerInput();
            _input = new CNetObj_PlayerInput();

            _ninja = new NinjaStats();
            g_Config = CConfiguration.Instance;
            _core = new CCharacterCore();
            _aWeapons = new WeaponStat[(int)Consts.NUM_WEAPONS];

            for (int i = 0; i < _aWeapons.Length; i++)
                _aWeapons[i] = new WeaponStat();
        }

        public override void Reset()
        {
            Destroy();
        }

        public CCharacterCore GetCore()
        {
            return _core;
        }

        public vec2 Velocity()
        {
            return _core.m_Vel;
        }

        public vec2 CorePosition()
        {
            return _core.m_Pos;
        }

        public vec2 CursorPosition()
        {
            return _core.m_Pos + new vec2(_input.m_TargetX, _input.m_TargetY);
        }

        public bool Spawn(CPlayer pPlayer, vec2 Pos)
        {
            _emoteStop = -1;
            _lastAction = -1;
            _activeWeapon = (int)Consts.WEAPON_GUN;
            _lastWeapon = (int)Consts.WEAPON_HAMMER;
            _queuedWeapon = -1;

            _player = pPlayer;
            m_Pos = Pos;

            _core.Reset();
            _core.Init(GameWorld, GameServer);
            _core.m_Pos = m_Pos;
            GameServer.m_World.m_apCharacters[_player.GetCID()] = _core;

            _reckoningTick = 0;
            _sendCore = new CCharacterCore();
            _reckoningCore = new CCharacterCore();

            GameServer.m_World.InsertEntity(this);
            GameServer.Controller.OnCharacterSpawn(this);
            _alive = true;
            _prevTile = (MapItems)GameServer.Collision.GetTileAtPos(m_Pos).m_Index;
            return true;
        }

        public override void Destroy()
        {
            GameServer.m_World.m_apCharacters[_player.GetCID()] = null;
            _alive = false;
        }

        public override void Tick()
        {
            _input.Write(_core.m_Input);
            _core.Tick(true);

            // handle death-tiles and leaving gamelayer
            if ((GameServer.Collision.GetCollisionAt(m_Pos.x + m_ProximityRadius / 3.0f, m_Pos.y - m_ProximityRadius / 3.0f) & CCollision.COLFLAG_DEATH) != 0 ||
                (GameServer.Collision.GetCollisionAt(m_Pos.x + m_ProximityRadius / 3.0f, m_Pos.y + m_ProximityRadius / 3.0f) & CCollision.COLFLAG_DEATH) != 0 ||
                (GameServer.Collision.GetCollisionAt(m_Pos.x - m_ProximityRadius / 3.0f, m_Pos.y - m_ProximityRadius / 3.0f) & CCollision.COLFLAG_DEATH) != 0 ||
                (GameServer.Collision.GetCollisionAt(m_Pos.x - m_ProximityRadius / 3.0f, m_Pos.y + m_ProximityRadius / 3.0f) & CCollision.COLFLAG_DEATH) != 0 ||
                (GameLayerClipped(m_Pos)))
            {
                Die(_player.GetCID(), WEAPON_WORLD);
            }

            // handle Weapons
            HandleWeapons();

            // Previnput
            _input.Write(_prevInput);

            var currentTile = (MapItems)GameServer.Collision.GetTileAtPos(m_Pos).m_Index;
            _prevTile = currentTile;

            bool isGrounded = IsGrounded();
            if (!_prevIsGrounded && isGrounded)
            {
                //GameServer.SendChatTarget(_player.GetCID(), _prevVel.ToString());
            }
            _prevIsGrounded = isGrounded;
            _prevVel = _core.m_Vel;
        }

        public int Direction()
        {
            return _input.m_Direction;
        }

        public override void TickDefered()
        {
            // advance the dummy
            {
                CGameWorld world = new CGameWorld();
                _reckoningCore.Init(world, GameServer);
                _reckoningCore.Tick(false);
                _reckoningCore.Move();
                _reckoningCore.Quantize();
            }

            //lastsentcore
            vec2 StartPos = _core.m_Pos;
            vec2 StartVel = _core.m_Vel;
            bool StuckBefore = GameServer.Collision.TestBox(_core.m_Pos, new vec2(28.0f, 28.0f));

            _core.Move();
            bool StuckAfterMove = GameServer.Collision.TestBox(_core.m_Pos, new vec2(28.0f, 28.0f));
            _core.Quantize();
            bool StuckAfterQuant = GameServer.Collision.TestBox(_core.m_Pos, new vec2(28.0f, 28.0f));
            m_Pos = _core.m_Pos;

            if (!StuckBefore && (StuckAfterMove || StuckAfterQuant))
            {
                //GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", "STUCK");
                // Hackish solution to get rid of strict-aliasing warning
                /*union
                {
                    float f;
                    unsigned u;
                }
                StartPosX, StartPosY, StartVelX, StartVelY;

                StartPosX.f = StartPos.x;
                StartPosY.f = StartPos.y;
                StartVelX.f = StartVel.x;
                StartVelY.f = StartVel.y;

                char aBuf[256];
                str_format(aBuf, sizeof(aBuf), "STUCK!!! %d %d %d %f %f %f %f %x %x %x %x",
                    StuckBefore,
                    StuckAfterMove,
                    StuckAfterQuant,
                    StartPos.x, StartPos.y,
                    StartVel.x, StartVel.y,
                    StartPosX.u, StartPosY.u,
                    StartVelX.u, StartVelY.u);
                GameServer.Console.Print(IConsole::OUTPUT_LEVEL_DEBUG, "game", aBuf);*/
            }

            int Events = _core.m_TriggeredEvents;
            int Mask = CGameContext.CmaskAllExceptOne(_player.GetCID());

            if ((Events & (int)GCEnum.COREEVENT_GROUND_JUMP) != 0)
                GameServer.CreateSound(m_Pos, (int)Consts.SOUND_PLAYER_JUMP, Mask);
            if ((Events & (int)GCEnum.COREEVENT_HOOK_ATTACH_PLAYER) != 0)
                GameServer.CreateSound(m_Pos, (int)Consts.SOUND_HOOK_ATTACH_PLAYER, CGameContext.CmaskAll());
            if ((Events & (int)GCEnum.COREEVENT_HOOK_ATTACH_GROUND) != 0)
                GameServer.CreateSound(m_Pos, (int)Consts.SOUND_HOOK_ATTACH_GROUND, Mask);
            if ((Events & (int)GCEnum.COREEVENT_HOOK_HIT_NOHOOK) != 0)
                GameServer.CreateSound(m_Pos, (int)Consts.SOUND_HOOK_NOATTACH, Mask);


            if (_player.GetTeam() == (int)Consts.TEAM_SPECTATORS)
            {
                m_Pos.x = _input.m_TargetX;
                m_Pos.y = _input.m_TargetY;
            }

            // update the m_SendCore if needed
            CNetObj_Character Predicted = new CNetObj_Character();
            CNetObj_Character Current = new CNetObj_Character();

            _reckoningCore.Write(Predicted);
            _core.Write(Current);

            // only allow dead reackoning for a top of 3 seconds
            if (_core.m_pReset || _reckoningTick + Server.TickSpeed() * 3 < Server.Tick() || !Predicted.Compare(Current))
            {
                _reckoningTick = Server.Tick();
                _core.Write(_sendCore);
                _core.Write(_reckoningCore);
                _core.m_pReset = false;
                //GameServer.SendChatTarget(_player.GetCID(), "UPDATE CORE");
            }
        }

        public override void TickPaused()
        {
            ++_attackTick;
            ++_damageTakenTick;
            ++_ninja.m_ActivationTick;
            ++_reckoningTick;

            if (_lastAction != -1)
                ++_lastAction;

            if (_aWeapons[_activeWeapon].m_AmmoRegenStart > -1)
                ++_aWeapons[_activeWeapon].m_AmmoRegenStart;

            if (_emoteStop > -1)
                ++_emoteStop;
        }

        public override void Snap(int SnappingClient)
        {
            int id = _player.GetCID();

            if (!Server.Translate(ref id, SnappingClient))
                return;

            if (NetworkClipped(SnappingClient))
                return;

            CNetObj_Character pCharacter = Server.SnapNetObj<CNetObj_Character>((int)Consts.NETOBJTYPE_CHARACTER, id);
         
            if (pCharacter == null)
                return;

            // write down the m_Core
            if (_reckoningTick == 0 || GameServer.m_World.m_Paused)
            {
                // no dead reckoning when paused because the client doesn't know
                // how far to perform the reckoning
                pCharacter.m_Tick = 0;
                _core.Write(pCharacter);
            }
            else
            {
                pCharacter.m_Tick = _reckoningTick;
                _sendCore.Write(pCharacter);
            }

            // set emote
            if (_emoteStop < Server.Tick())
            {
                _emoteType = (int)Consts.EMOTE_NORMAL;
                _emoteStop = -1;
            }

            if (pCharacter.m_HookedPlayer != -1)
            {
                if (!Server.Translate(ref pCharacter.m_HookedPlayer, SnappingClient))
                    pCharacter.m_HookedPlayer = -1;
            }

            pCharacter.m_AmmoCount = 0;
            pCharacter.m_Health = 0;
            pCharacter.m_Armor = 0;

            pCharacter.m_Weapon = _activeWeapon;
            pCharacter.m_AttackTick = _attackTick;
            pCharacter.m_Direction = _input.m_Direction;
            
            if (_player.GetCID() == SnappingClient || SnappingClient == -1 ||
                (g_Config.GetInt("SvStrictSpectateMode") == 0 && _player.GetCID() == GameServer.m_apPlayers[SnappingClient].m_SpectatorID))
            {
                pCharacter.m_Health = _health;
                pCharacter.m_Armor = _armor;
                if (_aWeapons[_activeWeapon].m_Ammo > 0)
                    pCharacter.m_AmmoCount = _aWeapons[_activeWeapon].m_Ammo;
            }

            /*
                EMOTE_NORMAL = 0,
                EMOTE_PAIN,
                EMOTE_HAPPY,
                EMOTE_SURPRISE,
                EMOTE_ANGRY,
                EMOTE_BLINK, 
            */

            if (_emoteType == (int)Consts.EMOTE_NORMAL)
            {
                if (Server.Tick()%(100+CSystem.random_uint()%250) == 0)
                {
                    _emoteType = (int) Consts.EMOTE_BLINK;
                    _emoteStop = Server.Tick() + 10;
                }
            }
            pCharacter.m_Emote = _emoteType;
            pCharacter.m_PlayerFlags = GetPlayer().m_PlayerFlags;
        }

        public bool IsGrounded()
        {
            if (GameServer.Collision.CheckPoint(m_Pos.x + m_ProximityRadius / 2, m_Pos.y + m_ProximityRadius / 2 + 5))
                return true;
            if (GameServer.Collision.CheckPoint(m_Pos.x - m_ProximityRadius / 2, m_Pos.y + m_ProximityRadius / 2 + 5))
                return true;
            return false;
        }

        public void SetWeapon(int W)
        {
            if (W == _activeWeapon)
                return;

            _lastWeapon = _activeWeapon;
            _queuedWeapon = -1;
            _activeWeapon = W;
            GameServer.CreateSound(m_Pos, (int)Consts.SOUND_WEAPON_SWITCH);

            if (_activeWeapon < 0 || _activeWeapon >= (int)Consts.NUM_WEAPONS)
                _activeWeapon = 0;
        }

        void HandleWeaponSwitch()
        {
            int WantedWeapon = _activeWeapon;
            if (_queuedWeapon != -1)
                WantedWeapon = _queuedWeapon;

            // select Weapon
            int Next = CountInput(_latestPrevInput.m_NextWeapon, _latestInput.m_NextWeapon).m_Presses;
            int Prev = CountInput(_latestPrevInput.m_PrevWeapon, _latestInput.m_PrevWeapon).m_Presses;

            if (Next < 128) // make sure we only try sane stuff
            {
                while (Next != 0) // Next Weapon selection
                {
                    WantedWeapon = (WantedWeapon + 1) % (int)Consts.NUM_WEAPONS;
                    if (_aWeapons[WantedWeapon].m_Got)
                        Next--;
                }
            }

            if (Prev < 128) // make sure we only try sane stuff
            {
                while (Prev != 0) // Prev Weapon selection
                {
                    WantedWeapon = (WantedWeapon - 1) < 0 ? (int)Consts.NUM_WEAPONS - 1 : WantedWeapon - 1;
                    if (_aWeapons[WantedWeapon].m_Got)
                        Prev--;
                }
            }

            // Direct Weapon selection
            if (_latestInput.m_WantedWeapon != 0)
                WantedWeapon = _input.m_WantedWeapon - 1;

            // check for insane values
            if (WantedWeapon >= 0 && WantedWeapon < (int)Consts.NUM_WEAPONS && WantedWeapon != _activeWeapon && _aWeapons[WantedWeapon].m_Got)
                _queuedWeapon = WantedWeapon;

            DoWeaponSwitch();
        }

        void DoWeaponSwitch()
        {
            // make sure we can switch
            if (_reloadTimer != 0 || _queuedWeapon == -1 || _aWeapons[(int)Consts.WEAPON_NINJA].m_Got)
                return;

            // switch Weapon
            SetWeapon(_queuedWeapon);
        }

        void HandleWeapons()
        {
            //ninja
            HandleNinja();

            // check reload timer
            if (_reloadTimer != 0)
            {
                _reloadTimer--;
                return;
            }

            // fire Weapon, if wanted
            FireWeapon();

            // ammo regen
            int AmmoRegenTime = g_pData.m_Weapons.m_aId[_activeWeapon].m_Ammoregentime;
            if (AmmoRegenTime != 0)
            {
                // If equipped and not active, regen ammo?
                if (_reloadTimer <= 0)
                {
                    if (_aWeapons[_activeWeapon].m_AmmoRegenStart < 0)
                        _aWeapons[_activeWeapon].m_AmmoRegenStart = Server.Tick();

                    if ((Server.Tick() - _aWeapons[_activeWeapon].m_AmmoRegenStart) >= AmmoRegenTime * Server.TickSpeed() / 1000)
                    {
                        // Add some ammo
                        _aWeapons[_activeWeapon].m_Ammo = Math.Min(_aWeapons[_activeWeapon].m_Ammo + 1, 10);
                        _aWeapons[_activeWeapon].m_AmmoRegenStart = -1;
                    }
                }
                else
                {
                    _aWeapons[_activeWeapon].m_AmmoRegenStart = -1;
                }
            }
        }

        void HandleNinja()
        {
            if (_activeWeapon != (int)Consts.WEAPON_NINJA)
                return;

            if ((Server.Tick() - _ninja.m_ActivationTick) > (g_pData.m_Weapons.m_Ninja.m_Duration * Server.TickSpeed() / 1000))
            {
                // time's up, return
                _aWeapons[(int)Consts.WEAPON_NINJA].m_Got = false;
                _activeWeapon = _lastWeapon;

                SetWeapon(_activeWeapon);
                return;
            }

            // force ninja Weapon
            SetWeapon((int)Consts.WEAPON_NINJA);

            _ninja.m_CurrentMoveTime--;

            if (_ninja.m_CurrentMoveTime == 0)
            {
                // reset velocity
                _core.m_Vel = _ninja.m_ActivationDir * _ninja.m_OldVelAmount;
            }

            if (_ninja.m_CurrentMoveTime > 0)
            {
                // Set velocity
                _core.m_Vel = _ninja.m_ActivationDir * g_pData.m_Weapons.m_Ninja.m_Velocity;
                vec2 OldPos = m_Pos;
                GameServer.Collision.MoveBox(ref _core.m_Pos, ref _core.m_Vel, new vec2(m_ProximityRadius, m_ProximityRadius), 0.0f);

                // reset velocity so the client doesn't predict stuff
                _core.m_Vel = new vec2(0.0f, 0.0f);

                // check if we Hit anything along the way
                {
                    CEntity[] aEnts = new CEntity[(int)Consts.MAX_CLIENTS];
                    vec2 Dir = m_Pos - OldPos;
                    float Radius = m_ProximityRadius * 2.0f;
                    vec2 Center = OldPos + Dir * 0.5f;
                    int Num = GameServer.m_World.FindEntities(Center, Radius, ref aEnts, (int)Consts.MAX_CLIENTS, CGameWorld.ENTTYPE_CHARACTER);

                    for (int i = 0; i < Num; ++i)
                    {
                        if (aEnts[i] == this)
                            continue;

                        // make sure we haven't Hit this object before
                        bool bAlreadyHit = false;
                        for (int j = 0; j < _numObjectsHit; j++)
                        {
                            if (_apHitObjects[j] == aEnts[i])
                                bAlreadyHit = true;
                        }
                        if (bAlreadyHit)
                            continue;

                        // check so we are sufficiently close
                        if (VMath.distance(aEnts[i].m_Pos, m_Pos) > (m_ProximityRadius * 2.0f))
                            continue;

                        // Hit a player, give him damage and stuffs...
                        GameServer.CreateSound(aEnts[i].m_Pos, (int)Consts.SOUND_NINJA_HIT);
                        // set his velocity to fast upward (for now)
                        if (_numObjectsHit < 10)
                            _apHitObjects[_numObjectsHit++] = aEnts[i];

                        ((CCharacter)aEnts[i]).TakeDamage(new vec2(0, -10.0f), g_pData.m_Weapons.m_Ninja.m_pBase.m_Damage, _player.GetCID(), (int)Consts.WEAPON_NINJA);
                    }
                }
            }
        }

        public void OnPredictedInput(CNetObj_PlayerInput pNewInput)
        {
            // check for changes
            if (!_input.Compare(pNewInput))
                _lastAction = Server.Tick();

            // copy new input
            pNewInput.Write(_input);
            _numInputs++;

            // it is not allowed to aim in the center
            if (_input.m_TargetX == 0 && _input.m_TargetY == 0)
                _input.m_TargetY = -1;
        }

        public void OnDirectInput(CNetObj_PlayerInput pNewInput)
        {
            _latestInput.Write(_latestPrevInput);
            pNewInput.Write(_latestInput);

            // it is not allowed to aim in the center
            if (_latestInput.m_TargetX == 0 && _latestInput.m_TargetY == 0)
                _latestInput.m_TargetY = -1;

            if (_numInputs > 2 && _player.GetTeam() != (int)Consts.TEAM_SPECTATORS)
            {
                HandleWeaponSwitch();
                FireWeapon();
            }

            _latestInput.Write(_latestPrevInput);
        }

        public void ResetInput()
        {
            _input.m_Direction = 0;
            _input.m_Hook = 0;
            // simulate releasing the fire button
            if ((_input.m_Fire & 1) != 0)
                _input.m_Fire++;
            _input.m_Fire &= (int)Consts.INPUT_STATE_MASK;
            _input.m_Jump = 0;

            _input.Write(_latestPrevInput);
            _input.Write(_latestInput);
        }

        //input count
        struct CInputCount
        {
            public int m_Presses;
            public int m_Releases;
        }

        CInputCount CountInput(int Prev, int Cur)
        {
            CInputCount c = new CInputCount();

            Prev &= (int)Consts.INPUT_STATE_MASK;
            Cur &= (int)Consts.INPUT_STATE_MASK;
            int i = Prev;

            while (i != Cur)
            {
                i = (i + 1) & (int)Consts.INPUT_STATE_MASK;
                if ((i & 1) != 0)
                    c.m_Presses++;
                else
                    c.m_Releases++;
            }

            return c;
        }

        public vec2 TargetPos()
        {
            return new vec2(_latestInput.m_TargetX, _latestInput.m_TargetY);
        }

        public int ActiveWeapon()
        {
            return _activeWeapon;
        }
        
        void FireWeapon()
        {
            if (_reloadTimer != 0)
                return;

            DoWeaponSwitch();
            
            bool FullAuto = false;
            if(_activeWeapon == (int)Consts.WEAPON_GRENADE || _activeWeapon == (int)Consts.WEAPON_SHOTGUN || _activeWeapon == (int)Consts.WEAPON_RIFLE)
                FullAuto = true;

            // check if we gonna fire
            bool WillFire = false;
            if (CountInput(_latestPrevInput.m_Fire, _latestInput.m_Fire).m_Presses != 0)
                WillFire = true;

            if (FullAuto && (_latestInput.m_Fire & 1) != 0 && _aWeapons[_activeWeapon].m_Ammo != 0)
                WillFire = true;

            if (!WillFire)
                return;

            // check for ammo
            if (_aWeapons[_activeWeapon].m_Ammo == 0)
            {
                // 125ms is a magical limit of how fast a human can click
                _reloadTimer = 125 * Server.TickSpeed() / 1000;
                if (_lastNoAmmoSound + Server.TickSpeed() <= Server.Tick())
                {
                    GameServer.CreateSound(m_Pos, (int)Consts.SOUND_WEAPON_NOAMMO);
                    _lastNoAmmoSound = Server.Tick();
                }
                return;
            }

            vec2 Direction = VMath.normalize(new vec2(_latestInput.m_TargetX, _latestInput.m_TargetY));
            vec2 ProjStartPos = m_Pos + Direction * m_ProximityRadius * 0.75f;
            
            switch (_activeWeapon)
            {
                case (int)Consts.WEAPON_HAMMER:
                {
                    // reset objects Hit
                    _numObjectsHit = 0;
                    GameServer.CreateSound(m_Pos, (int)Consts.SOUND_HAMMER_FIRE);

                    CEntity[] apEnts = new CEntity[(int)Consts.MAX_CLIENTS];
                    int Hits = 0;
                    int Num = GameServer.m_World.FindEntities(ProjStartPos, m_ProximityRadius * 0.5f, ref apEnts,
                        (int)Consts.MAX_CLIENTS, CGameWorld.ENTTYPE_CHARACTER);

                    for (int i = 0; i < Num; ++i)
                    {
                        CCharacter pTarget = (CCharacter)apEnts[i];
                        vec2 pOut1, pOut2;
                        if ((pTarget == this) || GameServer.Collision.IntersectLine(ProjStartPos, pTarget.m_Pos, out pOut1, out pOut2) != 0)
                            continue;

                        // set his velocity to fast upward (for now)
                        if (VMath.length(pTarget.m_Pos - ProjStartPos) > 0.0f)
                            GameServer.CreateHammerHit(pTarget.m_Pos - VMath.normalize(pTarget.m_Pos - ProjStartPos) * m_ProximityRadius * 0.5f);
                        else
                            GameServer.CreateHammerHit(ProjStartPos);

                        vec2 Dir;
                        if (VMath.length(pTarget.m_Pos - m_Pos) > 0.0f)
                            Dir = VMath.normalize(pTarget.m_Pos - m_Pos);
                        else
                            Dir = new vec2(0.0f, -1.0f);

                        pTarget.TakeDamage(new vec2(0.0f, -1.0f) + VMath.normalize(Dir + new vec2(0.0f, -1.1f)) * 10.0f, g_pData.m_Weapons.m_Hammer.m_pBase.m_Damage,
                            _player.GetCID(), _activeWeapon);
                        Hits++;
                    }

                    // if we Hit anything, we have to wait for the reload
                    if (Hits != 0)
                        _reloadTimer = Server.TickSpeed() / 3;
                }
                break;
                case (int)Consts.WEAPON_GUN:
                {
                    CProjectile pProj = new CProjectile(GameWorld, (int)Consts.WEAPON_GUN,
                        _player.GetCID(),
                        ProjStartPos,
                        Direction,
                        (int)(Server.TickSpeed() * GameServer.Tuning["GunLifetime"]),
                        1, false, 0, -1, (int)Consts.WEAPON_GUN);

                    // pack the Projectile and send it to the client Directly
                    CNetObj_Projectile p = new CNetObj_Projectile();
                    pProj.FillInfo(p);

                    CMsgPacker Msg = new CMsgPacker((int)Consts.NETMSGTYPE_SV_EXTRAPROJECTILE);
                    Msg.AddInt(1);
                    p.Fill(Msg);

                    Server.SendMsg(Msg, 0, _player.GetCID());
                    GameServer.CreateSound(m_Pos, (int)Consts.SOUND_GUN_FIRE);
                }
                break;
                case (int)Consts.WEAPON_SHOTGUN:
                {
                    int ShotSpread = 2;

                    CMsgPacker Msg = new CMsgPacker((int)Consts.NETMSGTYPE_SV_EXTRAPROJECTILE);
                    Msg.AddInt(ShotSpread * 2 + 1);

                    for (int i = -ShotSpread; i <= ShotSpread; ++i)
                    {
                        float[] Spreading = new float[] { -0.185f, -0.070f, 0, 0.070f, 0.185f };
                        float a = GCHelpers.GetAngle(Direction);
                        a += Spreading[i + 2];
                        float v = 1 - (Math.Abs(i) / (float)ShotSpread);
                        float Speed = CMath.mix(GameServer.Tuning["ShotgunSpeeddiff"], 1.0f, v);

                        CProjectile pProj = new CProjectile(GameWorld, (int)Consts.WEAPON_SHOTGUN,
                            _player.GetCID(),
                            ProjStartPos,
                            new vec2((float)Math.Cos(a), (float)Math.Sin(a)) * Speed,
                            (int)(Server.TickSpeed() * GameServer.Tuning["ShotgunLifetime"]),
                            1, false, 0, -1, (int)Consts.WEAPON_SHOTGUN);

                        // pack the Projectile and send it to the client Directly
                        CNetObj_Projectile p = new CNetObj_Projectile();
                        pProj.FillInfo(p);
                        p.Fill(Msg);
                    }

                    Server.SendMsg(Msg, 0, _player.GetCID());

                    GameServer.CreateSound(m_Pos, (int)Consts.SOUND_SHOTGUN_FIRE);
                }
                break;
                case (int)Consts.WEAPON_GRENADE:
                {
                    CProjectile pProj = new CProjectile(GameWorld, (int)Consts.WEAPON_GRENADE,
                        _player.GetCID(), ProjStartPos, Direction,
                        (int)(Server.TickSpeed()*GameServer.Tuning["GrenadeLifetime"]),
                        1, true, 0, (int)Consts.SOUND_GRENADE_EXPLODE, (int)Consts.WEAPON_GRENADE);

                    // pack the Projectile and send it to the client Directly
                    CNetObj_Projectile p = new CNetObj_Projectile();
                    pProj.FillInfo(p);

                    CMsgPacker Msg = new CMsgPacker((int)Consts.NETMSGTYPE_SV_EXTRAPROJECTILE);
                    Msg.AddInt(1);
                    p.Fill(Msg);

                    Server.SendMsg(Msg, 0, _player.GetCID());
                    GameServer.CreateSound(m_Pos, (int)Consts.SOUND_GRENADE_FIRE);
                }
                break;
                case (int)Consts.WEAPON_NINJA:
                {
                    // reset Hit objects
                    _numObjectsHit = 0;

                    _ninja.m_ActivationDir = Direction;
                    _ninja.m_CurrentMoveTime = g_pData.m_Weapons.m_Ninja.m_Movetime * Server.TickSpeed() / 1000;
                    _ninja.m_OldVelAmount = (int) VMath.length(_core.m_Vel);

                    GameServer.CreateSound(m_Pos, (int)Consts.SOUND_NINJA_FIRE);
                }
                break;
                    
            }

            _attackTick = Server.Tick();

            if (_aWeapons[_activeWeapon].m_Ammo > 0) // -1 == unlimited
                _aWeapons[_activeWeapon].m_Ammo--;

            if (_reloadTimer == 0)
                _reloadTimer = g_pData.m_Weapons.m_aId[_activeWeapon].m_Firedelay * Server.TickSpeed() / 1000;
        }

        public void Die(int Killer, int Weapon)
        {
            // we got to wait 0.5 secs before respawning
            _player.m_RespawnTick = Server.Tick() + Server.TickSpeed() / 2;
            int ModeSpecial = GameServer.Controller.OnCharacterDeath(this, GameServer.m_apPlayers[Killer], Weapon);

            string aBuf = string.Format("kill killer='{0}:{1}' victim='{2}:{3}' weapon={4} special={5}",
                Killer, Server.ClientName(Killer), _player.GetCID(), 
                Server.ClientName(_player.GetCID()), Weapon, ModeSpecial);
            GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", aBuf);

            // send the kill message
            CNetMsg_Sv_KillMsg Msg = new CNetMsg_Sv_KillMsg();
            Msg.m_Killer = Killer;
            Msg.m_Victim = _player.GetCID();
            Msg.m_Weapon = Weapon;
            Msg.m_ModeSpecial = ModeSpecial;
            Server.SendPackMsg(Msg, (int)Consts.MSGFLAG_VITAL, -1);

            // a nice sound
            GameServer.CreateSound(m_Pos, (int)Consts.SOUND_PLAYER_DIE);

            // this is for auto respawn after 3 secs
            _player.m_DieTick = Server.Tick();

            _alive = false;
            GameServer.m_World.RemoveEntity(this);
            GameServer.m_World.m_apCharacters[_player.GetCID()] = null;
            GameServer.CreateDeath(m_Pos, _player.GetCID());
        }

        public bool TakeDamage(vec2 Force, int Dmg, int From, int Weapon)
        {
            _core.m_Vel += Force;

            if (GameServer.Controller.IsFriendlyFire(_player.GetCID(), From) && g_Config.GetInt("SvTeamdamage") == 0)
                return false;

            // m_pPlayer only inflicts half damage on self
            if (From == _player.GetCID())
                Dmg = Math.Max(1, Dmg / 2);

            _damageTaken++;

            // create healthmod indicator
            if (Server.Tick() < _damageTakenTick + 25)
            {
                // make sure that the damage indicators doesn't group together
                GameServer.CreateDamageInd(m_Pos, _damageTaken * 0.25f, Dmg);
            }
            else
            {
                _damageTaken = 0;
                GameServer.CreateDamageInd(m_Pos, 0, Dmg);
            }

            if (Dmg != 0)
            {
                if (_armor != 0)
                {
                    if (Dmg > 1)
                    {
                        _health--;
                        Dmg--;
                    }

                    if (Dmg > _armor)
                    {
                        Dmg -= _armor;
                        _armor = 0;
                    }
                    else
                    {
                        _armor -= Dmg;
                        Dmg = 0;
                    }
                }

                _health -= Dmg;
            }

            _damageTakenTick = Server.Tick();

            // do damage Hit sound
            if (From >= 0 && From != _player.GetCID() && GameServer.m_apPlayers[From] != null)
            {
                int Mask = CGameContext.CmaskOne(From);
                for (int i = 0; i < (int)Consts.MAX_CLIENTS; i++)
                {
                    if (GameServer.m_apPlayers[i] != null && GameServer.m_apPlayers[i].GetTeam() == (int)Consts.TEAM_SPECTATORS && GameServer.m_apPlayers[i].m_SpectatorID == From)
                        Mask |= CGameContext.CmaskOne(i);
                }
                GameServer.CreateSound(GameServer.m_apPlayers[From].m_ViewPos, (int)Consts.SOUND_HIT, Mask);
            }

            // check for death
            if (_health <= 0)
            {
                Die(From, Weapon);

                // set attacker's face to happy (taunt!)
                if (From >= 0 && From != _player.GetCID() && GameServer.m_apPlayers[From] != null)
                {
                    CCharacter pChr = GameServer.m_apPlayers[From].GetCharacter();
                    if (pChr != null)
                    {
                        pChr._emoteType = (int)Consts.EMOTE_HAPPY;
                        pChr._emoteStop = Server.Tick() + Server.TickSpeed();
                    }
                }

                return false;
            }

            if (Dmg > 2)
                GameServer.CreateSound(m_Pos, (int)Consts.SOUND_PLAYER_PAIN_LONG);
            else
                GameServer.CreateSound(m_Pos, (int)Consts.SOUND_PLAYER_PAIN_SHORT);

            _emoteType = (int)Consts.EMOTE_PAIN;
            _emoteStop = Server.Tick() + 500 * Server.TickSpeed() / 1000;

            return true;
        }

        public bool IncreaseHealth(int Amount)
        {
            if (_health >= 10)
                return false;
            _health = CMath.clamp(_health + Amount, 0, 10);
            return true;
        }

        public bool IncreaseArmor(int Amount)
        {
            if (_armor >= 10)
                return false;
            _armor = CMath.clamp(_armor + Amount, 0, 10);
            return true;
        }

        public bool GiveWeapon(int Weapon, int Ammo)
        {
            if (_aWeapons[Weapon].m_Ammo < g_pData.m_Weapons.m_aId[Weapon].m_Maxammo || !_aWeapons[Weapon].m_Got)
            {
                _aWeapons[Weapon].m_Got = true;
                _aWeapons[Weapon].m_Ammo = Math.Min(g_pData.m_Weapons.m_aId[Weapon].m_Maxammo, Ammo);
                return true;
            }
            return false;
        }

        public void GiveNinja()
        {
            _ninja.m_ActivationTick = Server.Tick();
            _aWeapons[(int)Consts.WEAPON_NINJA].m_Got = true;
            _aWeapons[(int)Consts.WEAPON_NINJA].m_Ammo = -1;
            if (_activeWeapon != (int)Consts.WEAPON_NINJA)
                _lastWeapon = _activeWeapon;
            _activeWeapon = (int)Consts.WEAPON_NINJA;

            GameServer.CreateSound(m_Pos, (int)Consts.SOUND_PICKUP_NINJA);
        }

        public void SetEmote(int Emote, int Tick)
        {
            _emoteType = Emote;
            _emoteStop = Tick;
        }
    }
}
