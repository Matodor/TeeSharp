using System;

namespace TeeSharp
{
    public enum Weapons
    {
        HAMMER = 0,
        GUN,
        SHOTGUN,
        GRENADE,
        RIFLE,
        NINJA,

        NUM_WEAPONS,
    }

    public enum Emoticons
    {
        OOP = 0,
        EXCLAMATION,
        HEARTS,
        DROP,
        DOTDOT,
        MUSIC,
        SORRY,
        GHOST,
        SUSHI,
        SPLATTEE,
        DEVILTEE,
        ZOMG,
        ZZZ,
        WTF,
        EYES,
        QUESTION,

        NUM_EMOTICONS,
    }

    public enum Teams
    {
        SPECTATORS = -1,
        RED,
        BLUE,
    }

    public enum Emotes
    {
        NORMAL = 0,
        PAIN,
        HAPPY,
        SURPRISE,
        ANGRY,
        BLINK,

        NUM_EMOTES,
    }

    public enum FlagStates
    {
        MISSING = -3,
        ATSTAND,
        TAKEN,
    }

    public enum Powerups
    {
        POWERUP_HEALTH = 0,
        POWERUP_ARMOR,
        POWERUP_WEAPON,
        POWERUP_NINJA,

        NUM_POWERUPS,
    }

    [Flags]
    public enum PlayerFlags
    {
        PLAYING = 1 << 0,
        PLAYERFLAG_IN_MENU = 1 << 1,
        PLAYERFLAG_CHATTING = 1 << 2,
        PLAYERFLAG_SCOREBOARD = 1 << 3,
    }

    [Flags]
    public enum GameFlags
    {
        TEAMS = 1 << 0,
        FLAGS = 1 << 1,
    }

    [Flags]
    public enum GameStateFlags
    {
        GAMEOVER = 1 << 0,
        SUDDENDEATH = 1 << 1,
        PAUSED = 1 << 2,
    }

    public enum NetObjTypes
    {
        NETOBJ_INVALID = 0,
        NETOBJ_PLAYERINPUT,
        NETOBJ_PROJECTILE,
        NETOBJ_LASER,
        NETOBJ_PICKUP,
        NETOBJ_FLAG,
        NETOBJ_GAMEINFO,
        NETOBJ_GAMEDATA,
        NETOBJ_CHARACTERCORE,
        NETOBJ_CHARACTER,
        NETOBJ_PLAYERINFO,
        NETOBJ_CLIENTINFO,
        NETOBJ_SPECTATORINFO,

        NETEVENT_COMMON,
        NETEVENT_EXPLOSION,
        NETEVENT_SPAWN,
        NETEVENT_HAMMERHIT,
        NETEVENT_DEATH,
        NETEVENT_SOUNDGLOBAL,
        NETEVENT_SOUNDWORLD,
        NETEVENT_DAMAGEIND,

        NUM_NETOBJTYPES,
    }

    public enum NetMessages
    {
        INVALID = 0,
        SV_MOTD,
        SV_BROADCAST,
        SV_CHAT,
        SV_KILLMSG,
        SV_SOUNDGLOBAL,
        SV_TUNEPARAMS,
        SV_EXTRAPROJECTILE,
        SV_READYTOENTER,
        SV_WEAPONPICKUP,
        SV_EMOTICON,
        SV_VOTECLEAROPTIONS,
        SV_VOTEOPTIONLISTADD,
        SV_VOTEOPTIONADD,
        SV_VOTEOPTIONREMOVE,
        SV_VOTESET,
        SV_VOTESTATUS,
        CL_SAY,
        CL_SETTEAM,
        CL_SETSPECTATORMODE,
        CL_STARTINFO,
        CL_CHANGEINFO,
        CL_KILL,
        CL_EMOTICON,
        CL_VOTE,
        CL_CALLVOTE,
        CL_ISDDNET,
        NUM_NETMSGTYPES,

        NETMSG_NULL = 0,

        // the first thing sent by the client
        // contains the version info for the client
        NETMSG_INFO = 1,
        // sent by server
        NETMSG_MAP_CHANGE,      // sent when client should switch map
        NETMSG_MAP_DATA,        // map transfer, contains a chunk of the map file
        NETMSG_CON_READY,       // connection is ready, client should send start info
        NETMSG_SNAP,            // normal snapshot, multiple parts
        NETMSG_SNAPEMPTY,       // empty snapshot
        NETMSG_SNAPSINGLE,      // ?
        NETMSG_SNAPSMALL,       //
        NETMSG_INPUTTIMING,     // reports how off the input was
        NETMSG_RCON_AUTH_STATUS,// result of the authentication
        NETMSG_RCON_LINE,       // line that should be printed to the remote console

        NETMSG_AUTH_CHALLANGE,  //
        NETMSG_AUTH_RESULT,     //

        // sent by client
        NETMSG_READY,           //
        NETMSG_ENTERGAME,
        NETMSG_INPUT,           // contains the inputdata from the client
        NETMSG_RCON_CMD,        //
        NETMSG_RCON_AUTH,       //
        NETMSG_REQUEST_MAP_DATA,//

        NETMSG_AUTH_START,      //
        NETMSG_AUTH_RESPONSE,   //

        // sent by both
        NETMSG_PING,
        NETMSG_PING_REPLY,
        NETMSG_ERROR,

        NETMSG_RCON_CMD_ADD,
        NETMSG_RCON_CMD_REM,
    }

    public enum Sounds
    {
        GUN_FIRE = 0,
        SHOTGUN_FIRE,
        GRENADE_FIRE,
        HAMMER_FIRE,
        HAMMER_HIT,
        NINJA_FIRE,
        GRENADE_EXPLODE,
        NINJA_HIT,
        RIFLE_FIRE,
        RIFLE_BOUNCE,
        WEAPON_SWITCH,
        PLAYER_PAIN_SHORT,
        PLAYER_PAIN_LONG,
        BODY_LAND,
        PLAYER_AIRJUMP,
        PLAYER_JUMP,
        PLAYER_DIE,
        PLAYER_SPAWN,
        PLAYER_SKID,
        TEE_CRY,
        HOOK_LOOP,
        HOOK_ATTACH_GROUND,
        HOOK_ATTACH_PLAYER,
        HOOK_NOATTACH,
        PICKUP_HEALTH,
        PICKUP_ARMOR,
        PICKUP_GRENADE,
        PICKUP_SHOTGUN,
        PICKUP_NINJA,
        WEAPON_SPAWN,
        WEAPON_NOAMMO,
        HIT,
        CHAT_SERVER,
        CHAT_CLIENT,
        CHAT_HIGHLIGHT,
        CTF_DROP,
        CTF_RETURN,
        CTF_GRAB_PL,
        CTF_GRAB_EN,
        CTF_CAPTURE,
        MENU,

        NUM_SOUNDS,
    }

    public enum ChatTeams
    {
        CHAT_ALL = -2,
        CHAT_SPEC = -1,
        CHAT_RED = 0,
        CHAT_BLUE = 1
    }

    [Flags]
    public enum MsgFlags
    {
        VITAL = 1 << 0,
        FLUSH = 1 << 1,
        NORECORD = 1 << 2,
        RECORD = 1 << 3,
        NOSEND = 1 << 4,
    }

    public partial class Consts
    {
        public const int
            INPUT_STATE_MASK = 0b0111111,
            SPEC_FREEVIEW = -1,

            SERVER_TICK_SPEED = 50,
            SERVER_FLAG_PASSWORD = 0b00000001,

            MAX_CLIENTS = 64,
            VANILLA_MAX_CLIENTS = 16,

            MAX_INPUT_SIZE = 128,
            MAX_SNAPSHOT_PACKSIZE = 900,

            MAX_NAME_LENGTH = 16,
            MAX_CLAN_LENGTH = 12,

            VERSION_VANILLA = 0,
            VERSION_DDRACE = 1,
            VERSION_DDNET_OLD = 2,
            VERSION_DDNET_WHISPER = 217,
            VERSION_DDNET_GOODHOOK = 221,
            VERSION_DDNET_EXTRATUNES = 302;
    }
}
