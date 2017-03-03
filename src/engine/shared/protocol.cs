using System;

namespace Teecsharp
{
    public class CNetObj_PlayerInput
    {
        public int m_Direction;
        public int m_TargetX;
        public int m_TargetY;
        public int m_Jump;
        public int m_Fire;
        public int m_Hook;
        public int m_PlayerFlags;
        public int m_WantedWeapon;
        public int m_NextWeapon;
        public int m_PrevWeapon;

        public bool Compare(CNetObj_PlayerInput other)
        {
            return
                m_Direction == other.m_Direction &&
                m_TargetX == other.m_TargetX &&
                m_TargetY == other.m_TargetY &&
                m_Jump == other.m_Jump &&
                m_Fire == other.m_Fire &&
                m_Hook == other.m_Hook &&
                m_PlayerFlags == other.m_PlayerFlags &&
                m_WantedWeapon == other.m_WantedWeapon &&
                m_NextWeapon == other.m_NextWeapon &&
                m_PrevWeapon == other.m_PrevWeapon;
        }

        public void Write(CNetObj_PlayerInput output)
        {
            output.m_Direction = m_Direction;
            output.m_TargetX = m_TargetX;
            output.m_TargetY = m_TargetY;
            output.m_Jump = m_Jump;
            output.m_Fire = m_Fire;
            output.m_Hook = m_Hook;
            output.m_PlayerFlags = m_PlayerFlags;
            output.m_WantedWeapon = m_WantedWeapon;
            output.m_NextWeapon = m_NextWeapon;
            output.m_PrevWeapon = m_PrevWeapon;
        }

        public void Read(int[] data)
        {
            m_Direction = data[0];
            m_TargetX = data[1];
            m_TargetY = data[2];
            m_Jump = data[3];
            m_Fire = data[4];
            m_Hook = data[5];
            m_PlayerFlags = data[6];
            m_WantedWeapon = data[7];
            m_NextWeapon = data[8];
            m_PrevWeapon = data[9];
        }
    }

    public class CNetObj_Projectile : CNet_Common
    {
        public int m_X;
        public int m_Y;
        public int m_VelX;
        public int m_VelY;
        public int m_Type;
        public int m_StartTick;

        public void Fill(CMsgPacker Msg)
        {
            Msg.AddInt(m_X);
            Msg.AddInt(m_Y);
            Msg.AddInt(m_VelX);
            Msg.AddInt(m_VelY);
            Msg.AddInt(m_Type);
            Msg.AddInt(m_StartTick);
        }

        public override int[] GetArray()
        {
            return new[]
            {
                m_X,
                m_Y,
                m_VelX,
                m_VelY,
                m_Type,
                m_StartTick
            };
        }
    }

    public class CNetObj_Laser : CNet_Common
    {
        public int m_X;
        public int m_Y;
        public int m_FromX;
        public int m_FromY;
        public int m_StartTick;

        public override int[] GetArray()
        {
            return new[]
            {
                m_X,
                m_Y,
                m_FromX,
                m_FromY,
                m_StartTick
            };
        }
    }

    public class CNetObj_Pickup : CNet_Common
    {
        public int m_X;
        public int m_Y;
        public int m_Type;
        public int m_Subtype;

        public override int[] GetArray()
        {
            return new[]
            {
                m_X,
                m_Y,
                m_Type,
                m_Subtype,
            };
        }
    }

    public class CNetObj_Flag : CNet_Common
    {
        public int m_X;
        public int m_Y;
        public int m_Team;

        public override int[] GetArray()
        {
            return new[]
            {
                m_X,
                m_Y,
                m_Team,
            };
        }
    }

    public class CNetObj_GameInfo : CNet_Common
    {
        public int m_GameFlags;
        public int m_GameStateFlags;
        public int m_RoundStartTick;
        public int m_WarmupTimer;
        public int m_ScoreLimit;
        public int m_TimeLimit;
        public int m_RoundNum;
        public int m_RoundCurrent;

        public override int[] GetArray()
        {
            return new[]
            {
                m_GameFlags,
                m_GameStateFlags,
                m_RoundStartTick,
                m_WarmupTimer,
                m_ScoreLimit,
                m_TimeLimit,
                m_RoundNum,
                m_RoundCurrent,
            };
        }
    }

    public class CNetObj_GameData : CNet_Common
    {
        public int m_TeamscoreRed;
        public int m_TeamscoreBlue;
        public int m_FlagCarrierRed;
        public int m_FlagCarrierBlue;

        public override int[] GetArray()
        {
            return new[]
            {
                m_TeamscoreRed,
                m_TeamscoreBlue,
                m_FlagCarrierRed,
                m_FlagCarrierBlue,
            };
        }
    }

    public class CNetObj_Character : CNet_Common
    {
        public int m_Tick;
        public int m_X;
        public int m_Y;
        public int m_VelX;
        public int m_VelY;
        public int m_Angle;
        public int m_Direction;
        public int m_Jumped;
        public int m_HookedPlayer;
        public int m_HookState;
        public int m_HookTick;
        public int m_HookX;
        public int m_HookY;
        public int m_HookDx;
        public int m_HookDy;

        public int m_PlayerFlags;
        public int m_Health;
        public int m_Armor;
        public int m_AmmoCount;
        public int m_Weapon;
        public int m_Emote;
        public int m_AttackTick;

        public bool Compare(CNetObj_Character other)
        {
            return
                m_Tick == other.m_Tick &&
                m_X == other.m_X &&
                m_Y == other.m_Y &&
                m_VelX == other.m_VelX &&
                m_VelY == other.m_VelY &&
                m_Angle == other.m_Angle &&
                m_Direction == other.m_Direction &&
                m_Jumped == other.m_Jumped &&
                m_HookedPlayer == other.m_HookedPlayer &&
                m_HookState == other.m_HookState &&
                m_HookTick == other.m_HookTick &&
                m_HookX == other.m_HookX &&
                m_HookY == other.m_HookY &&
                m_HookDx == other.m_HookDx &&
                m_HookDy == other.m_HookDy &&

                m_PlayerFlags == other.m_PlayerFlags &&
                m_Health == other.m_Health &&
                m_Armor == other.m_Armor &&
                m_AmmoCount == other.m_AmmoCount &&
                m_Weapon == other.m_Weapon &&
                m_Emote == other.m_Emote &&
                m_AttackTick == other.m_AttackTick;
        }

        public override int[] GetArray()
        {
            return new[]
            {
                m_Tick,
                m_X,
                m_Y,
                m_VelX,
                m_VelY,
                m_Angle,
                m_Direction,
                m_Jumped,
                m_HookedPlayer,
                m_HookState,
                m_HookTick,
                m_HookX,
                m_HookY,
                m_HookDx,
                m_HookDy,

                m_PlayerFlags,
                m_Health,
                m_Armor,
                m_AmmoCount,
                m_Weapon,
                m_Emote,
                m_AttackTick,
            };
        }
    }

    public class CNetObj_PlayerInfo : CNet_Common
    {
        public int m_Local;
        public int m_ClientID;
        public int m_Team;
        public int m_Score;
        public int m_Latency;

        public override int[] GetArray()
        {
            return new[]
            {
                m_Local,
                m_ClientID,
                m_Team,
                m_Score,
                m_Latency,
            };
        }
    }

    public class CNetObj_ClientInfo : CNet_Common
    {
        public int m_Name0;
        public int m_Name1;
        public int m_Name2;
        public int m_Name3;
        public int m_Clan0;
        public int m_Clan1;
        public int m_Clan2;
        public int m_Country;
        public int m_Skin0;
        public int m_Skin1;
        public int m_Skin2;
        public int m_Skin3;
        public int m_Skin4;
        public int m_Skin5;
        public int m_UseCustomColor;
        public int m_ColorBody;
        public int m_ColorFeet;

        public override int[] GetArray()
        {
            return new[]
            {
                m_Name0,
                m_Name1,
                m_Name2,
                m_Name3,
                m_Clan0,
                m_Clan1,
                m_Clan2,
                m_Country,
                m_Skin0,
                m_Skin1,
                m_Skin2,
                m_Skin3,
                m_Skin4,
                m_Skin5,
                m_UseCustomColor,
                m_ColorBody,
                m_ColorFeet,
            };
        }
    }

    public class CNetObj_SpectatorInfo : CNet_Common
    {
        public int m_SpectatorID;
        public int m_X;
        public int m_Y;

        public override int[] GetArray()
        {
            return new[]
            {
                m_SpectatorID,
                m_X,
                m_Y,
            };
        }
    }

    public abstract class CNet_Common
    {
        public abstract int[] GetArray();
    }

    public abstract class CNetEvent_Common : CNet_Common
    {
        public abstract vector2_float Pos(); 
        public abstract void Write(CNetEvent_Common output);
    }

    public class CNetEvent_Explosion : CNetEvent_Common
    {
        public int m_X;
        public int m_Y;

        public override int[] GetArray()
        {
            return new[]
            {
                m_X,
                m_Y
            };
        }

        public override vector2_float Pos()
        {
            return new vector2_float(m_X, m_Y);
        }

        public override void Write(CNetEvent_Common output)
        {
            var obj = (CNetEvent_Explosion) output;
            obj.m_X = m_X;
            obj.m_Y = m_Y;
        }
    }

    public class CNetEvent_Spawn : CNetEvent_Common
    {
        public int m_X;
        public int m_Y;

        public override int[] GetArray()
        {
            return new[]
            {
                m_X,
                m_Y
            };
        }

        public override vector2_float Pos()
        {
            return new vector2_float(m_X, m_Y);
        }

        public override void Write(CNetEvent_Common output)
        {
            var obj = (CNetEvent_Spawn)output;
            obj.m_X = m_X;
            obj.m_Y = m_Y;
        }
    }

    public class CNetEvent_HammerHit : CNetEvent_Common
    {
        public int m_X;
        public int m_Y;

        public override int[] GetArray()
        {
            return new[]
            {
                m_X,
                m_Y
            };
        }

        public override vector2_float Pos()
        {
            return new vector2_float(m_X, m_Y);
        }

        public override void Write(CNetEvent_Common output)
        {
            var obj = (CNetEvent_HammerHit)output;
            obj.m_X = m_X;
            obj.m_Y = m_Y;
        }
    }

    public class CNetEvent_Death : CNetEvent_Common
    {
        public int m_X;
        public int m_Y;
        public int m_ClientID;

        public override int[] GetArray()
        {
            return new[]
            {
                m_X,
                m_Y,
                m_ClientID
            };
        }

        public override vector2_float Pos()
        {
            return new vector2_float(m_X, m_Y);
        }

        public override void Write(CNetEvent_Common output)
        {
            var obj = (CNetEvent_Death)output;
            obj.m_X = m_X;
            obj.m_Y = m_Y;
            obj.m_ClientID = m_ClientID;
        }
    }

    public class CNetEvent_SoundGlobal : CNetEvent_Common
    {
        public int m_X;
        public int m_Y;
        public int m_SoundID;

        public override int[] GetArray()
        {
            return new[]
            {
                m_X,
                m_Y,
                m_SoundID
            };
        }

        public override vector2_float Pos()
        {
            return new vector2_float(m_X, m_Y);
        }

        public override void Write(CNetEvent_Common output)
        {
            var obj = (CNetEvent_SoundGlobal)output;
            obj.m_X = m_X;
            obj.m_Y = m_Y;
            obj.m_SoundID = m_SoundID;
        }
    }

    class CNetEvent_SoundWorld : CNetEvent_Common
    {
        public int m_X;
        public int m_Y;
        public int m_SoundID;

        public override int[] GetArray()
        {
            return new[]
            {
                m_X,
                m_Y,
                m_SoundID
            };
        }

        public override vector2_float Pos()
        {
            return new vector2_float(m_X, m_Y);
        }

        public override void Write(CNetEvent_Common output)
        {
            var obj = (CNetEvent_SoundWorld)output;
            obj.m_X = m_X;
            obj.m_Y = m_Y;
            obj.m_SoundID = m_SoundID;
        }
    }

    public class CNetEvent_DamageInd : CNetEvent_Common
    {
        public int m_X;
        public int m_Y;
        public int m_Angle;

        public override int[] GetArray()
        {
            return new[]
            {
                m_X,
                m_Y,
                m_Angle
            };
        }

        public override vector2_float Pos()
        {
            return new vector2_float(m_X, m_Y);
        }

        public override void Write(CNetEvent_Common output)
        {
            var obj = (CNetEvent_DamageInd)output;
            obj.m_X = m_X;
            obj.m_Y = m_Y;
            obj.m_Angle = m_Angle;
        }
    }

    public abstract class CNetMsgBase
    {
        public abstract void Write(CNetMsgBase output);
        public abstract int MsgID();
        public abstract bool Pack(CMsgPacker pPacker);
    }

    public class CNetMsg_Sv_Motd : CNetMsgBase
    {
        public string m_pMessage;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_Motd) output;
            obj.m_pMessage = m_pMessage;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_MOTD; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddString(m_pMessage, -1);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_Broadcast : CNetMsgBase
    {
        public string m_pMessage;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_Broadcast)output;
            obj.m_pMessage = m_pMessage;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_BROADCAST; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddString(m_pMessage, -1);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_Chat : CNetMsgBase
    {
        public int m_Team;
        public int m_ClientID;
        public string m_pMessage;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_Chat)output;
            obj.m_Team = m_Team;
            obj.m_ClientID = m_ClientID;
            obj.m_pMessage = m_pMessage;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_CHAT; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_Team);
            pPacker.AddInt(m_ClientID);
            pPacker.AddString(m_pMessage, -1);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_KillMsg : CNetMsgBase
    {
        public int m_Killer;
        public int m_Victim;
        public int m_Weapon;
        public int m_ModeSpecial;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_KillMsg)output;
            obj.m_Killer = m_Killer;
            obj.m_Victim = m_Victim;
            obj.m_Weapon = m_Weapon;
            obj.m_ModeSpecial = m_ModeSpecial;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_KILLMSG; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_Killer);
            pPacker.AddInt(m_Victim);
            pPacker.AddInt(m_Weapon);
            pPacker.AddInt(m_ModeSpecial);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_SoundGlobal : CNetMsgBase
    {
        public int m_SoundID;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_SoundGlobal)output;
            obj.m_SoundID = m_SoundID;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_SOUNDGLOBAL; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_SoundID);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_TuneParams : CNetMsgBase
    {
        public override void Write(CNetMsgBase output) { }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_TUNEPARAMS; }

        public override bool Pack(CMsgPacker pPacker)
        {
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_ExtraProjectile : CNetMsgBase
    {
        public override void Write(CNetMsgBase output) { }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_EXTRAPROJECTILE; }

        public override bool Pack(CMsgPacker pPacker)
        {
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_ReadyToEnter : CNetMsgBase
    {
        public override void Write(CNetMsgBase output) { }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_READYTOENTER; }

        public override bool Pack(CMsgPacker pPacker)
        {
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_WeaponPickup : CNetMsgBase
    {
        public int m_Weapon;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_WeaponPickup)output;
            obj.m_Weapon = m_Weapon;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_WEAPONPICKUP; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_Weapon);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_Emoticon : CNetMsgBase
    {
        public int m_ClientID;
        public int m_Emoticon;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_Emoticon)output;
            obj.m_ClientID = m_ClientID;
            obj.m_Emoticon = m_Emoticon;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_EMOTICON; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_ClientID);
            pPacker.AddInt(m_Emoticon);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_VoteClearOptions : CNetMsgBase
    {
        public override void Write(CNetMsgBase output) { }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_VOTECLEAROPTIONS; }

        public override bool Pack(CMsgPacker pPacker)
        {
            return pPacker.Error();
        }
    }
    public class CNetMsg_Sv_VoteOptionListAdd : CNetMsgBase
    {
        public int m_NumOptions;
        public string m_pDescription0;
        public string m_pDescription1;
        public string m_pDescription2;
        public string m_pDescription3;
        public string m_pDescription4;
        public string m_pDescription5;
        public string m_pDescription6;
        public string m_pDescription7;
        public string m_pDescription8;
        public string m_pDescription9;
        public string m_pDescription10;
        public string m_pDescription11;
        public string m_pDescription12;
        public string m_pDescription13;
        public string m_pDescription14;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_VoteOptionListAdd)output;
            obj.m_NumOptions = m_NumOptions;
            obj.m_pDescription0 = m_pDescription0;
            obj.m_pDescription1 = m_pDescription1;
            obj.m_pDescription2 = m_pDescription2;
            obj.m_pDescription3 = m_pDescription3;
            obj.m_pDescription4 = m_pDescription4;
            obj.m_pDescription5 = m_pDescription5;
            obj.m_pDescription6 = m_pDescription6;
            obj.m_pDescription7 = m_pDescription7;
            obj.m_pDescription8 = m_pDescription8;
            obj.m_pDescription9 = m_pDescription9;
            obj.m_pDescription10 = m_pDescription10;
            obj.m_pDescription11 = m_pDescription11;
            obj.m_pDescription12 = m_pDescription12;
            obj.m_pDescription13 = m_pDescription13;
            obj.m_pDescription14 = m_pDescription14;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_VOTEOPTIONLISTADD; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_NumOptions);
            pPacker.AddString(m_pDescription0, -1);
            pPacker.AddString(m_pDescription1, -1);
            pPacker.AddString(m_pDescription2, -1);
            pPacker.AddString(m_pDescription3, -1);
            pPacker.AddString(m_pDescription4, -1);
            pPacker.AddString(m_pDescription5, -1);
            pPacker.AddString(m_pDescription6, -1);
            pPacker.AddString(m_pDescription7, -1);
            pPacker.AddString(m_pDescription8, -1);
            pPacker.AddString(m_pDescription9, -1);
            pPacker.AddString(m_pDescription10, -1);
            pPacker.AddString(m_pDescription11, -1);
            pPacker.AddString(m_pDescription12, -1);
            pPacker.AddString(m_pDescription13, -1);
            pPacker.AddString(m_pDescription14, -1);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_VoteOptionAdd : CNetMsgBase
    {
        public string m_pDescription;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_VoteOptionAdd)output;
            obj.m_pDescription = m_pDescription;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_VOTEOPTIONADD; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddString(m_pDescription, -1);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_VoteOptionRemove : CNetMsgBase
    {
        public string m_pDescription;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_VoteOptionRemove)output;
            obj.m_pDescription = m_pDescription;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_VOTEOPTIONREMOVE; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddString(m_pDescription, -1);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_VoteSet : CNetMsgBase
    {
        public int m_Timeout;
        public string m_pDescription;
        public string m_pReason;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_VoteSet)output;
            obj.m_Timeout = m_Timeout;
            obj.m_pDescription = m_pDescription;
            obj.m_pReason = m_pReason;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_VOTESET; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_Timeout);
            pPacker.AddString(m_pDescription, -1);
            pPacker.AddString(m_pReason, -1);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Sv_VoteStatus : CNetMsgBase
    {
        public int m_Yes;
        public int m_No;
        public int m_Pass;
        public int m_Total;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Sv_VoteStatus)output;
            obj.m_Yes = m_Yes;
            obj.m_No = m_No;
            obj.m_Pass = m_Pass;
            obj.m_Total = m_Total;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_SV_VOTESTATUS; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_Yes);
            pPacker.AddInt(m_No);
            pPacker.AddInt(m_Pass);
            pPacker.AddInt(m_Total);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Cl_Say : CNetMsgBase
    {
        public int m_Team;
        public string m_pMessage;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Cl_Say)output;
            obj.m_Team = m_Team;
            obj.m_pMessage = m_pMessage;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_CL_SAY; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_Team);
            pPacker.AddString(m_pMessage, -1);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Cl_SetTeam : CNetMsgBase
    {
        public int m_Team;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Cl_SetTeam)output;
            obj.m_Team = m_Team;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_CL_SETTEAM; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_Team);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Cl_SetSpectatorMode : CNetMsgBase
    {
        public int m_SpectatorID;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Cl_SetSpectatorMode)output;
            obj.m_SpectatorID = m_SpectatorID;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_CL_SETSPECTATORMODE; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_SpectatorID);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Cl_StartInfo : CNetMsgBase
    {
        public string m_pName;
        public string m_pClan;
        public int m_Country;
        public string m_pSkin;
        public int m_UseCustomColor;
        public int m_ColorBody;
        public int m_ColorFeet;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Cl_StartInfo)output;
            obj.m_pName = m_pName;
            obj.m_pClan = m_pClan;
            obj.m_Country = m_Country;
            obj.m_pSkin = m_pSkin;
            obj.m_UseCustomColor = m_UseCustomColor;
            obj.m_ColorBody = m_ColorBody;
            obj.m_ColorFeet = m_ColorFeet;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_CL_STARTINFO; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddString(m_pName, -1);
            pPacker.AddString(m_pClan, -1);
            pPacker.AddInt(m_Country);
            pPacker.AddString(m_pSkin, -1);
            pPacker.AddInt(m_UseCustomColor);
            pPacker.AddInt(m_ColorBody);
            pPacker.AddInt(m_ColorFeet);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Cl_ChangeInfo : CNetMsgBase
    {
        public string m_pName;
        public string m_pClan;
        public int m_Country;
        public string m_pSkin;
        public int m_UseCustomColor;
        public int m_ColorBody;
        public int m_ColorFeet;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Cl_ChangeInfo)output;
            obj.m_pName = m_pName;
            obj.m_pClan = m_pClan;
            obj.m_Country = m_Country;
            obj.m_pSkin = m_pSkin;
            obj.m_UseCustomColor = m_UseCustomColor;
            obj.m_ColorBody = m_ColorBody;
            obj.m_ColorFeet = m_ColorFeet;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_CL_CHANGEINFO; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddString(m_pName, -1);
            pPacker.AddString(m_pClan, -1);
            pPacker.AddInt(m_Country);
            pPacker.AddString(m_pSkin, -1);
            pPacker.AddInt(m_UseCustomColor);
            pPacker.AddInt(m_ColorBody);
            pPacker.AddInt(m_ColorFeet);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Cl_Kill : CNetMsgBase
    {
        public override void Write(CNetMsgBase output) { }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_CL_KILL; }

        public override bool Pack(CMsgPacker pPacker)
        {
            return pPacker.Error();
        }
    }

    public class CNetMsg_Cl_Emoticon : CNetMsgBase
    {
        public int m_Emoticon;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Cl_Emoticon)output;
            obj.m_Emoticon = m_Emoticon;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_CL_EMOTICON; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_Emoticon);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Cl_Vote : CNetMsgBase
    {
        public int m_Vote;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Cl_Vote)output;
            obj.m_Vote = m_Vote;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_CL_VOTE; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddInt(m_Vote);
            return pPacker.Error();
        }
    }

    public class CNetMsg_Cl_CallVote : CNetMsgBase
    {
        public string m_Type;
        public string m_Value;
        public string m_Reason;

        public override void Write(CNetMsgBase output)
        {
            var obj = (CNetMsg_Cl_CallVote)output;
            obj.m_Type = m_Type;
            obj.m_Value = m_Value;
            obj.m_Reason = m_Reason;
        }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_CL_CALLVOTE; }

        public override bool Pack(CMsgPacker pPacker)
        {
            pPacker.AddString(m_Type, -1);
            pPacker.AddString(m_Value, -1);
            pPacker.AddString(m_Reason, -1);
            return pPacker.Error();
        }
    }
    public class CNetMsg_Cl_IsDDNet : CNetMsgBase
    {
        public override void Write(CNetMsgBase output) { }

        public override int MsgID() { return (int)Consts.NETMSGTYPE_CL_ISDDNET; }

        public override bool Pack(CMsgPacker pPacker)
        {
            return pPacker.Error();
        }
    }

    public class CNetObjHandler
    {
        public int NumObjCorrections() { return m_NumObjCorrections; }
        public string CorrectedObjOn() { return m_pObjCorrectedOn; }
        public string FailedMsgOn() { return m_pMsgFailedOn; }

        public string m_pMsgFailedOn;
        public string m_pObjCorrectedOn;
        public int m_NumObjCorrections;

        /*private static int[] ms_aObjSizes = {
            0,
            Marshal.SizeOf<CNetObj_PlayerInput>(),
            Marshal.SizeOf<CNetObj_Projectile>(),
            Marshal.SizeOf<CNetObj_Laser>(),
            Marshal.SizeOf<CNetObj_Pickup>(),
            Marshal.SizeOf<CNetObj_Flag>(),
            Marshal.SizeOf<CNetObj_GameInfo>(),
            Marshal.SizeOf<CNetObj_GameData>(),
            Marshal.SizeOf<CNetObj_CharacterCore>(),
            Marshal.SizeOf<CNetObj_Character>(),
            Marshal.SizeOf<CNetObj_PlayerInfo>(),
            Marshal.SizeOf<CNetObj_ClientInfo>(),
            Marshal.SizeOf<CNetObj_SpectatorInfo>(),
            Marshal.SizeOf<CNetEvent_Common>(),
            Marshal.SizeOf<CNetEvent_Explosion>(),
            Marshal.SizeOf<CNetEvent_Spawn>(),
            Marshal.SizeOf<CNetEvent_HammerHit>(),
            Marshal.SizeOf<CNetEvent_Death>(),
            Marshal.SizeOf<CNetEvent_SoundGlobal>(),
            Marshal.SizeOf<CNetEvent_SoundWorld>(),
            Marshal.SizeOf<CNetEvent_DamageInd>(),
            0
        };*/

        private static readonly string[] ms_apMsgNames = {
            "invalid",
            "Sv_Motd",
            "Sv_Broadcast",
            "Sv_Chat",
            "Sv_KillMsg",
            "Sv_SoundGlobal",
            "Sv_TuneParams",
            "Sv_ExtraProjectile",
            "Sv_ReadyToEnter",
            "Sv_WeaponPickup",
            "Sv_Emoticon",
            "Sv_VoteClearOptions",
            "Sv_VoteOptionListAdd",
            "Sv_VoteOptionAdd",
            "Sv_VoteOptionRemove",
            "Sv_VoteSet",
            "Sv_VoteStatus",
            "Cl_Say",
            "Cl_SetTeam",
            "Cl_SetSpectatorMode",
            "Cl_StartInfo",
            "Cl_ChangeInfo",
            "Cl_Kill",
            "Cl_Emoticon",
            "Cl_Vote",
            "Cl_CallVote",
            "Cl_IsDDNet",
            ""
        };

        private static readonly string[] ms_apObjNames = {
            "invalid",
            "PlayerInput",
            "Projectile",
            "Laser",
            "Pickup",
            "Flag",
            "GameInfo",
            "GameData",
            "CharacterCore",
            "Character",
            "PlayerInfo",
            "ClientInfo",
            "SpectatorInfo",
            "Common",
            "Explosion",
            "Spawn",
            "HammerHit",
            "Death",
            "SoundGlobal",
            "SoundWorld",
            "DamageInd",
            ""
        };

        ~CNetObjHandler()
        {
            //Marshal.FreeHGlobal(m_aMsgData);
        }

        public CNetObjHandler()
        {
            //m_aMsgData = Marshal.AllocHGlobal(1024);
            m_pMsgFailedOn = "";
            m_pObjCorrectedOn = "";
            m_NumObjCorrections = 0;
        }

        public int ClampInt(string pErrorMsg, int Value, int Min, int Max)
        {
            if (Value < Min) { m_pObjCorrectedOn = pErrorMsg; m_NumObjCorrections++; return Min; }
            if (Value > Max) { m_pObjCorrectedOn = pErrorMsg; m_NumObjCorrections++; return Max; }
            return Value;
        }

        string GetObjName(int Type)
        {
            if (Type < 0 || Type >= (int)Consts.NUM_NETOBJTYPES)
                return "(out of range)";
            return ms_apObjNames[Type];
        }

        /*public int GetObjSize(int Type)
        {
            if (Type < 0 || Type >= (int)Consts.NUM_NETOBJTYPES)
                return 0;
            return ms_aObjSizes[Type];
        }*/

        public string GetMsgName(int Type)
        {
            if (Type < 0 || Type >= (int)Consts.NUM_NETMSGTYPES)
                return "(out of range)";
            return ms_apMsgNames[Type];
        }

        /*public int ValidateObj(int Type, IntPtr pData, int Size)
        {
            switch (Type)
            {
                case (int)Consts.NETOBJTYPE_PLAYERINPUT:
                    {
                        CNetObj_PlayerInput pObj = CSystem.get_obj<CNetObj_PlayerInput>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_PlayerFlags", pObj.m_PlayerFlags, 0, 256);
                        return 0;
                    }

                case (int)Consts.NETOBJTYPE_PROJECTILE:
                    {
                        CNetObj_Projectile pObj = CSystem.get_obj<CNetObj_Projectile>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_Type", pObj.m_Type, 0, (int)Consts.NUM_WEAPONS - 1);
                        ClampInt("m_StartTick", pObj.m_StartTick, 0, max_int);
                        return 0;
                    }

                case (int)Consts.NETOBJTYPE_LASER:
                    {
                        CNetObj_Laser pObj = CSystem.get_obj<CNetObj_Laser>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_StartTick", pObj.m_StartTick, 0, max_int);
                        return 0;
                    }

                case (int)Consts.NETOBJTYPE_PICKUP:
                    {
                        CNetObj_Pickup pObj = CSystem.get_obj<CNetObj_Pickup>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_Type", pObj.m_Type, 0, max_int);
                        ClampInt("m_Subtype", pObj.m_Subtype, 0, max_int);
                        return 0;
                    }

                case (int)Consts.NETOBJTYPE_FLAG:
                    {
                        CNetObj_Flag pObj = CSystem.get_obj<CNetObj_Flag>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_Team", pObj.m_Team, (int)Consts.TEAM_RED, (int)Consts.TEAM_BLUE);
                        return 0;
                    }

                case (int)Consts.NETOBJTYPE_GAMEINFO:
                    {
                        CNetObj_GameInfo pObj = CSystem.get_obj<CNetObj_GameInfo>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_GameFlags", pObj.m_GameFlags, 0, 256);
                        ClampInt("m_GameStateFlags", pObj.m_GameStateFlags, 0, 256);
                        ClampInt("m_RoundStartTick", pObj.m_RoundStartTick, 0, max_int);
                        ClampInt("m_WarmupTimer", pObj.m_WarmupTimer, 0, max_int);
                        ClampInt("m_ScoreLimit", pObj.m_ScoreLimit, 0, max_int);
                        ClampInt("m_TimeLimit", pObj.m_TimeLimit, 0, max_int);
                        ClampInt("m_RoundNum", pObj.m_RoundNum, 0, max_int);
                        ClampInt("m_RoundCurrent", pObj.m_RoundCurrent, 0, max_int);
                        return 0;
                    }

                case (int)Consts.NETOBJTYPE_GAMEDATA:
                    {
                        CNetObj_GameData pObj = CSystem.get_obj<CNetObj_GameData>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_FlagCarrierRed", pObj.m_FlagCarrierRed, (int)Consts.FLAG_MISSING, (int)Consts.MAX_CLIENTS - 1);
                        ClampInt("m_FlagCarrierBlue", pObj.m_FlagCarrierBlue, (int)Consts.FLAG_MISSING, (int)Consts.MAX_CLIENTS - 1);
                        return 0;
                    }

                case (int)Consts.NETOBJTYPE_CHARACTERCORE:
                    {
                        CNetObj_CharacterCore pObj = CSystem.get_obj<CNetObj_CharacterCore>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_Direction", pObj.m_Direction, -1, 1);
                        ClampInt("m_Jumped", pObj.m_Jumped, 0, 3);
                        ClampInt("m_HookedPlayer", pObj.m_HookedPlayer, 0, (int)Consts.MAX_CLIENTS - 1);
                        ClampInt("m_HookState", pObj.m_HookState, -1, 5);
                        ClampInt("m_HookTick", pObj.m_HookTick, 0, max_int);
                        return 0;
                    }

                case (int)Consts.NETOBJTYPE_CHARACTER:
                    {
                        CNetObj_Character pObj = CSystem.get_obj<CNetObj_Character>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_PlayerFlags", pObj.m_PlayerFlags, 0, 256);
                        ClampInt("m_Health", pObj.m_Health, 0, 10);
                        ClampInt("m_Armor", pObj.m_Armor, 0, 10);
                        ClampInt("m_AmmoCount", pObj.m_AmmoCount, 0, 10);
                        ClampInt("m_Weapon", pObj.m_Weapon, 0, (int)Consts.NUM_WEAPONS - 1);
                        ClampInt("m_Emote", pObj.m_Emote, 0, 6);
                        ClampInt("m_AttackTick", pObj.m_AttackTick, 0, max_int);
                        return 0;
                    }

                case (int)Consts.NETOBJTYPE_PLAYERINFO:
                    {
                        CNetObj_PlayerInfo pObj = CSystem.get_obj<CNetObj_PlayerInfo>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_Local", pObj.m_Local, 0, 1);
                        ClampInt("m_ClientID", pObj.m_ClientID, 0, (int)Consts.MAX_CLIENTS - 1);
                        ClampInt("m_Team", pObj.m_Team, (int)Consts.TEAM_SPECTATORS, (int)Consts.TEAM_BLUE);
                        return 0;
                    }

                case (int)Consts.NETOBJTYPE_CLIENTINFO:
                    {
                        CNetObj_ClientInfo pObj = CSystem.get_obj<CNetObj_ClientInfo>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_UseCustomColor", pObj.m_UseCustomColor, 0, 1);
                        return 0;
                    }

                case (int)Consts.NETOBJTYPE_SPECTATORINFO:
                    {
                        CNetObj_SpectatorInfo pObj = CSystem.get_obj<CNetObj_SpectatorInfo>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_SpectatorID", pObj.m_SpectatorID, (int)Consts.SPEC_FREEVIEW, (int)Consts.MAX_CLIENTS - 1);
                        return 0;
                    }

                case (int)Consts.NETEVENTTYPE_COMMON:
                    {
                        CNetEvent_Common pObj = CSystem.get_obj<CNetEvent_Common>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        return 0;
                    }

                case (int)Consts.NETEVENTTYPE_EXPLOSION:
                    {
                        CNetEvent_Explosion pObj = CSystem.get_obj<CNetEvent_Explosion>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        return 0;
                    }

                case (int)Consts.NETEVENTTYPE_SPAWN:
                    {
                        CNetEvent_Spawn pObj = CSystem.get_obj<CNetEvent_Spawn>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        return 0;
                    }

                case (int)Consts.NETEVENTTYPE_HAMMERHIT:
                    {
                        CNetEvent_HammerHit pObj = CSystem.get_obj<CNetEvent_HammerHit>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        return 0;
                    }

                case (int)Consts.NETEVENTTYPE_DEATH:
                    {
                        CNetEvent_Death pObj = CSystem.get_obj<CNetEvent_Death>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_ClientID", pObj.m_ClientID, 0, (int)Consts.MAX_CLIENTS - 1);
                        return 0;
                    }

                case (int)Consts.NETEVENTTYPE_SOUNDGLOBAL:
                    {
                        CNetEvent_SoundGlobal pObj = CSystem.get_obj<CNetEvent_SoundGlobal>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_SoundID", pObj.m_SoundID, 0, (int)Consts.NUM_SOUNDS - 1);
                        return 0;
                    }

                case (int)Consts.NETEVENTTYPE_SOUNDWORLD:
                    {
                        CNetEvent_SoundWorld pObj = CSystem.get_obj<CNetEvent_SoundWorld>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        ClampInt("m_SoundID", pObj.m_SoundID, 0, (int)Consts.NUM_SOUNDS - 1);
                        return 0;
                    }

                case (int)Consts.NETEVENTTYPE_DAMAGEIND:
                    {
                        CNetEvent_DamageInd pObj = CSystem.get_obj<CNetEvent_DamageInd>(pData);
                        if (Marshal.SizeOf(pObj) != Size) return -1;
                        return 0;
                    }
            }
            return -1;
        }*/

        public bool SecureUnpackMsg(int Type, CUnpacker pUnpacker, ref object output)
        {
            m_pMsgFailedOn = null;

            switch (Type)
            {
                case (int)Consts.NETMSGTYPE_SV_MOTD:
                    {
                        CNetMsg_Sv_Motd pMsg = new CNetMsg_Sv_Motd();
                        pMsg.m_pMessage = pUnpacker.GetString();
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_BROADCAST:
                    {
                        CNetMsg_Sv_Broadcast pMsg = new CNetMsg_Sv_Broadcast();
                        pMsg.m_pMessage = pUnpacker.GetString();
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_CHAT:
                    {
                        CNetMsg_Sv_Chat pMsg = new CNetMsg_Sv_Chat();

                        pMsg.m_Team = pUnpacker.GetInt();
                        pMsg.m_ClientID = pUnpacker.GetInt();
                        pMsg.m_pMessage = pUnpacker.GetString();
                        if (pMsg.m_Team < (int)Consts.TEAM_SPECTATORS || pMsg.m_Team > (int)Consts.TEAM_BLUE) { m_pMsgFailedOn = "m_Team"; break; }
                        if (pMsg.m_ClientID < -1 || pMsg.m_ClientID > (int)Consts.MAX_CLIENTS - 1) { m_pMsgFailedOn = "m_ClientID"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_KILLMSG:
                    {
                        CNetMsg_Sv_KillMsg pMsg = new CNetMsg_Sv_KillMsg();

                        pMsg.m_Killer = pUnpacker.GetInt();
                        pMsg.m_Victim = pUnpacker.GetInt();
                        pMsg.m_Weapon = pUnpacker.GetInt();
                        pMsg.m_ModeSpecial = pUnpacker.GetInt();
                        if (pMsg.m_Killer < 0 || pMsg.m_Killer > (int)Consts.MAX_CLIENTS - 1) { m_pMsgFailedOn = "m_Killer"; break; }
                        if (pMsg.m_Victim < 0 || pMsg.m_Victim > (int)Consts.MAX_CLIENTS - 1) { m_pMsgFailedOn = "m_Victim"; break; }
                        if (pMsg.m_Weapon < -3 || pMsg.m_Weapon > (int)Consts.NUM_WEAPONS - 1) { m_pMsgFailedOn = "m_Weapon"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_SOUNDGLOBAL:
                    {
                        CNetMsg_Sv_SoundGlobal pMsg = new CNetMsg_Sv_SoundGlobal();

                        pMsg.m_SoundID = pUnpacker.GetInt();
                        if (pMsg.m_SoundID < 0 || pMsg.m_SoundID > (int)Consts.NUM_SOUNDS - 1) { m_pMsgFailedOn = "m_SoundID"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_TUNEPARAMS:
                    {
                        CNetMsg_Sv_TuneParams pMsg = new CNetMsg_Sv_TuneParams();
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_EXTRAPROJECTILE:
                    {
                        CNetMsg_Sv_ExtraProjectile pMsg = new CNetMsg_Sv_ExtraProjectile();
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_READYTOENTER:
                    {
                        CNetMsg_Sv_ReadyToEnter pMsg = new CNetMsg_Sv_ReadyToEnter();
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_WEAPONPICKUP:
                    {
                        CNetMsg_Sv_WeaponPickup pMsg = new CNetMsg_Sv_WeaponPickup();

                        pMsg.m_Weapon = pUnpacker.GetInt();
                        if (pMsg.m_Weapon < 0 || pMsg.m_Weapon > (int)Consts.NUM_WEAPONS - 1) { m_pMsgFailedOn = "m_Weapon"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_EMOTICON:
                    {
                        CNetMsg_Sv_Emoticon pMsg = new CNetMsg_Sv_Emoticon();

                        pMsg.m_ClientID = pUnpacker.GetInt();
                        pMsg.m_Emoticon = pUnpacker.GetInt();
                        if (pMsg.m_ClientID < 0 || pMsg.m_ClientID > (int)Consts.MAX_CLIENTS - 1) { m_pMsgFailedOn = "m_ClientID"; break; }
                        if (pMsg.m_Emoticon < 0 || pMsg.m_Emoticon > (int)Consts.NUM_EMOTICONS - 1) { m_pMsgFailedOn = "m_Emoticon"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_VOTECLEAROPTIONS:
                    {
                        CNetMsg_Sv_VoteClearOptions pMsg = new CNetMsg_Sv_VoteClearOptions();
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_VOTEOPTIONLISTADD:
                    {
                        CNetMsg_Sv_VoteOptionListAdd pMsg = new CNetMsg_Sv_VoteOptionListAdd();

                        pMsg.m_NumOptions = pUnpacker.GetInt();
                        pMsg.m_pDescription0 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription1 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription2 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription3 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription4 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription5 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription6 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription7 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription8 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription9 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription10 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription11 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription12 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription13 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pDescription14 = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        if (pMsg.m_NumOptions < 1 || pMsg.m_NumOptions > 15) { m_pMsgFailedOn = "m_NumOptions"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_VOTEOPTIONADD:
                    {
                        CNetMsg_Sv_VoteOptionAdd pMsg = new CNetMsg_Sv_VoteOptionAdd();
                        pMsg.m_pDescription = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_VOTEOPTIONREMOVE:
                    {
                        CNetMsg_Sv_VoteOptionRemove pMsg = new CNetMsg_Sv_VoteOptionRemove();
                        pMsg.m_pDescription = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_VOTESET:
                    {
                        CNetMsg_Sv_VoteSet pMsg = new CNetMsg_Sv_VoteSet();

                        pMsg.m_Timeout = pUnpacker.GetInt();
                        pMsg.m_pDescription = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pReason = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        if (pMsg.m_Timeout < 0 || pMsg.m_Timeout > 60) { m_pMsgFailedOn = "m_Timeout"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_SV_VOTESTATUS:
                    {
                        CNetMsg_Sv_VoteStatus pMsg = new CNetMsg_Sv_VoteStatus();

                        pMsg.m_Yes = pUnpacker.GetInt();
                        pMsg.m_No = pUnpacker.GetInt();
                        pMsg.m_Pass = pUnpacker.GetInt();
                        pMsg.m_Total = pUnpacker.GetInt();
                        if (pMsg.m_Yes < 0 || pMsg.m_Yes > (int)Consts.MAX_CLIENTS) { m_pMsgFailedOn = "m_Yes"; break; }
                        if (pMsg.m_No < 0 || pMsg.m_No > (int)Consts.MAX_CLIENTS) { m_pMsgFailedOn = "m_No"; break; }
                        if (pMsg.m_Pass < 0 || pMsg.m_Pass > (int)Consts.MAX_CLIENTS) { m_pMsgFailedOn = "m_Pass"; break; }
                        if (pMsg.m_Total < 0 || pMsg.m_Total > (int)Consts.MAX_CLIENTS) { m_pMsgFailedOn = "m_Total"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_CL_SAY:
                    {
                        CNetMsg_Cl_Say pMsg = new CNetMsg_Cl_Say();

                        pMsg.m_Team = pUnpacker.GetInt();
                        pMsg.m_pMessage = pUnpacker.GetString();
                        if (pMsg.m_Team < 0 || pMsg.m_Team > 1) { m_pMsgFailedOn = "m_Team"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_CL_SETTEAM:
                    {
                        CNetMsg_Cl_SetTeam pMsg = new CNetMsg_Cl_SetTeam();

                        pMsg.m_Team = pUnpacker.GetInt();
                        if (pMsg.m_Team < (int)Consts.TEAM_SPECTATORS || pMsg.m_Team > (int)Consts.TEAM_BLUE) { m_pMsgFailedOn = "m_Team"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_CL_SETSPECTATORMODE:
                    {
                        CNetMsg_Cl_SetSpectatorMode pMsg = new CNetMsg_Cl_SetSpectatorMode();

                        pMsg.m_SpectatorID = pUnpacker.GetInt();
                        if (pMsg.m_SpectatorID < (int)Consts.SPEC_FREEVIEW || pMsg.m_SpectatorID > (int)Consts.MAX_CLIENTS - 1) { m_pMsgFailedOn = "m_SpectatorID"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_CL_STARTINFO:
                    {
                        CNetMsg_Cl_StartInfo pMsg = new CNetMsg_Cl_StartInfo();

                        pMsg.m_pName = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pClan = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_Country = pUnpacker.GetInt();
                        pMsg.m_pSkin = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_UseCustomColor = pUnpacker.GetInt();
                        pMsg.m_ColorBody = pUnpacker.GetInt();
                        pMsg.m_ColorFeet = pUnpacker.GetInt();

                        if (pMsg.m_UseCustomColor < 0 || pMsg.m_UseCustomColor > 1)
                        {
                            m_pMsgFailedOn = "m_UseCustomColor"; break;
                        }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_CL_CHANGEINFO:
                    {
                        CNetMsg_Cl_ChangeInfo pMsg = new CNetMsg_Cl_ChangeInfo();

                        pMsg.m_pName = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_pClan = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_Country = pUnpacker.GetInt();
                        pMsg.m_pSkin = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_UseCustomColor = pUnpacker.GetInt();
                        pMsg.m_ColorBody = pUnpacker.GetInt();
                        pMsg.m_ColorFeet = pUnpacker.GetInt();
                        if (pMsg.m_UseCustomColor < 0 || pMsg.m_UseCustomColor > 1)
                        {
                            m_pMsgFailedOn = "m_UseCustomColor"; break;
                        }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_CL_KILL:
                    {
                        CNetMsg_Cl_Kill pMsg = new CNetMsg_Cl_Kill();
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_CL_EMOTICON:
                    {
                        CNetMsg_Cl_Emoticon pMsg = new CNetMsg_Cl_Emoticon();

                        pMsg.m_Emoticon = pUnpacker.GetInt();
                        if (pMsg.m_Emoticon < 0 || pMsg.m_Emoticon > (int)Consts.NUM_EMOTICONS - 1) { m_pMsgFailedOn = "m_Emoticon"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_CL_VOTE:
                    {
                        CNetMsg_Cl_Vote pMsg = new CNetMsg_Cl_Vote();

                        pMsg.m_Vote = pUnpacker.GetInt();
                        if (pMsg.m_Vote < -1 || pMsg.m_Vote > 1) { m_pMsgFailedOn = "m_Vote"; break; }
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_CL_CALLVOTE:
                    {
                        CNetMsg_Cl_CallVote pMsg = new CNetMsg_Cl_CallVote();

                        pMsg.m_Type = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_Value = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        pMsg.m_Reason = pUnpacker.GetString(CUnpacker.SANITIZE_CC | CUnpacker.SKIP_START_WHITESPACES);
                        output = pMsg;
                    }
                    break;

                case (int)Consts.NETMSGTYPE_CL_ISDDNET:
                    {
                        CNetMsg_Cl_IsDDNet pMsg = new CNetMsg_Cl_IsDDNet();
                        output = pMsg;
                    }
                    break;

                default:
                    m_pMsgFailedOn = "(type out of range)";
                    break;
            }

            if (pUnpacker.Error())
                m_pMsgFailedOn = "(unpack error)";

            if (!string.IsNullOrEmpty(m_pMsgFailedOn))
                return false;

            m_pMsgFailedOn = "";
            return true;
        }
    }
}
