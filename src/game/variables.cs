using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public partial class CConfiguration
    {
        private static readonly Dictionary<string, object> default_variablesDictionary = new Dictionary<string, object>
        {
            // client
            { "ClPredict", new ConfigInt("ClPredict", "cl_predict", 1, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Predict client movements") },
            { "ClNameplates", new ConfigInt("ClNameplates", "cl_nameplates", 1, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Show name plates") },
            { "ClNameplatesAlways", new ConfigInt("ClNameplatesAlways", "cl_nameplates_always", 1, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Always show name plates disregarding of distance") },
            { "ClNameplatesTeamcolors", new ConfigInt("ClNameplatesTeamcolors", "cl_nameplates_teamcolors", 1, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Use team colors for name plates") },
            { "ClNameplatesSize", new ConfigInt("ClNameplatesSize", "cl_nameplates_size", 50, 0, 100, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Size of the name plates from 0 to 100%") },
            { "ClAutoswitchWeapons", new ConfigInt("ClAutoswitchWeapons", "cl_autoswitch_weapons", 0, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Auto switch weapon on pickup") },

            { "ClShowhud", new ConfigInt("ClShowhud", "cl_showhud", 1, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Show ingame HUD") },
            { "ClShowChatFriends", new ConfigInt("ClShowChatFriends", "cl_show_chat_friends", 0, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Show only chat messages from friends") },
            { "ClShowfps", new ConfigInt("ClShowfps", "cl_showfps", 0, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Show ingame FPS counter") },

            { "ClAirjumpindicator", new ConfigInt("ClAirjumpindicator", "cl_airjumpindicator", 1, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "") },
            { "ClThreadsoundloading", new ConfigInt("ClThreadsoundloading", "cl_threadsoundloading", 0, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Load sound files threaded") },

            { "ClWarningTeambalance", new ConfigInt("ClWarningTeambalance", "cl_warning_teambalance", 1, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Warn about team balance") },

            { "ClMouseDeadzone", new ConfigInt("ClMouseDeadzone", "cl_mouse_deadzone", 300, 0, 0, CFGFLAG_CLIENT|CFGFLAG_SAVE, "") },
            { "ClMouseFollowfactor", new ConfigInt("ClMouseFollowfactor", "cl_mouse_followfactor", 60, 0, 200, CFGFLAG_CLIENT|CFGFLAG_SAVE, "") },
            { "ClMouseMaxDistance", new ConfigInt("ClMouseMaxDistance", "cl_mouse_max_distance", 800, 0, 0, CFGFLAG_CLIENT|CFGFLAG_SAVE, "") },

            { "EdShowkeys", new ConfigInt("EdShowkeys", "ed_showkeys", 0, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "") },

            { "ClShowWelcome", new ConfigInt("ClShowWelcome", "cl_show_welcome", 1, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "") },
            { "ClMotdTime", new ConfigInt("ClMotdTime", "cl_motd_time", 10, 0, 100, CFGFLAG_CLIENT|CFGFLAG_SAVE, "How long to show the server message of the day") },

            { "ClVersionServer", new ConfigStr("ClVersionServer", "cl_version_server", 100, "version.teeworlds.com", CFGFLAG_CLIENT|CFGFLAG_SAVE, "Server to use to check for new versions") },

            { "ClLanguagefile", new ConfigStr("ClLanguagefile", "cl_languagefile", 255, "", CFGFLAG_CLIENT|CFGFLAG_SAVE, "What language file to use") },

            { "PlayerUseCustomColor", new ConfigInt("PlayerUseCustomColor", "player_use_custom_color", 0, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Toggles usage of custom colors") },
            { "PlayerColorBody", new ConfigInt("PlayerColorBody", "player_color_body", 65408, 0, 0xFFFFFF, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Player body color") },
            { "PlayerColorFeet", new ConfigInt("PlayerColorFeet", "player_color_feet", 65408, 0, 0xFFFFFF, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Player feet color") },
            { "PlayerSkin", new ConfigStr("PlayerSkin", "player_skin", 24, "default", CFGFLAG_CLIENT|CFGFLAG_SAVE, "Player skin") },

            { "UiPage", new ConfigInt("UiPage", "ui_page", 6, 0, 10, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Interface page") },
            { "UiToolboxPage", new ConfigInt("UiToolboxPage", "ui_toolbox_page", 0, 0, 2, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Toolbox page") },
            { "UiServerAddress", new ConfigStr("UiServerAddress", "ui_server_address", 64, "localhost:8303", CFGFLAG_CLIENT|CFGFLAG_SAVE, "Interface server address") },
            { "UiScale", new ConfigInt("UiScale", "ui_scale", 100, 50, 150, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Interface scale") },
            { "UiMousesens", new ConfigInt("UiMousesens", "ui_mousesens", 100, 5, 100000, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Mouse sensitivity for menus/editor") },

            { "UiColorHue", new ConfigInt("UiColorHue", "ui_color_hue", 160, 0, 255, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Interface color hue") },
            { "UiColorSat", new ConfigInt("UiColorSat", "ui_color_sat", 70, 0, 255, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Interface color saturation") },
            { "UiColorLht", new ConfigInt("UiColorLht", "ui_color_lht", 175, 0, 255, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Interface color lightness") },
            { "UiColorAlpha", new ConfigInt("UiColorAlpha", "ui_color_alpha", 228, 0, 255, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Interface alpha") },

            { "GfxNoclip", new ConfigInt("GfxNoclip", "gfx_noclip", 0, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SAVE, "Disable clipping") },

            // server
            { "SvWarmup", new ConfigInt("SvWarmup", "sv_warmup", 0, 0, 0, CFGFLAG_SERVER, "Number of seconds to do warmup before round starts") },
            { "SvMotd", new ConfigStr("SvMotd", "sv_motd", 900, "", CFGFLAG_SERVER, "Message of the day to display for the clients") },
            { "SvTeamdamage", new ConfigInt("SvTeamdamage", "sv_teamdamage", 0, 0, 1, CFGFLAG_SERVER, "Team damage") },
            { "SvMaprotation", new ConfigStr("SvMaprotation", "sv_maprotation", 768, "", CFGFLAG_SERVER, "Maps to rotate between") },
            { "SvRoundsPerMap", new ConfigInt("SvRoundsPerMap", "sv_rounds_per_map", 1, 1, 100, CFGFLAG_SERVER, "Number of rounds on each map before rotating") },
            { "SvRoundSwap", new ConfigInt("SvRoundSwap", "sv_round_swap", 1, 0, 1, CFGFLAG_SERVER, "Swap teams between rounds") },
            { "SvPowerups", new ConfigInt("SvPowerups", "sv_powerups", 1, 0, 1, CFGFLAG_SERVER, "Allow powerups like ninja") },
            { "SvScorelimit", new ConfigInt("SvScorelimit", "sv_scorelimit", 20, 0, 1000, CFGFLAG_SERVER, "Score limit (0 disables)") },
            { "SvTimelimit", new ConfigInt("SvTimelimit", "sv_timelimit", 0, 0, 1000, CFGFLAG_SERVER, "Time limit in minutes (0 disables)") },
            { "SvGametype", new ConfigStr("SvGametype", "sv_gametype", 32, "dm", CFGFLAG_SERVER, "Game type (dm, tdm, ctf)") },
            { "SvTournamentMode", new ConfigInt("SvTournamentMode", "sv_tournament_mode", 0, 0, 1, CFGFLAG_SERVER, "Tournament mode. When enabled, players joins the server as spectator") },
            { "SvSpamprotection", new ConfigInt("SvSpamprotection", "sv_spamprotection", 1, 0, 1, CFGFLAG_SERVER, "Spam protection") },

            { "SvRespawnDelayTDM", new ConfigInt("SvRespawnDelayTDM", "sv_respawn_delay_tdm", 3, 0, 10, CFGFLAG_SERVER, "Time needed to respawn after death in tdm gametype") },

            { "SvSpectatorSlots", new ConfigInt("SvSpectatorSlots", "sv_spectator_slots", 0, 0, (int)Consts.MAX_CLIENTS, CFGFLAG_SERVER, "Number of slots to reserve for spectators") },
            { "SvTeambalanceTime", new ConfigInt("SvTeambalanceTime", "sv_teambalance_time", 1, 0, 1000, CFGFLAG_SERVER, "How many minutes to wait before autobalancing teams") },
            { "SvInactiveKickTime", new ConfigInt("SvInactiveKickTime", "sv_inactivekick_time", 3, 0, 1000, CFGFLAG_SERVER, "How many minutes to wait before taking care of inactive players") },
            { "SvInactiveKick", new ConfigInt("SvInactiveKick", "sv_inactivekick", 1, 0, 2, CFGFLAG_SERVER, "How to deal with inactive players (0=move to spectator, 1=move to free spectator slot/kick, 2=kick)") },

            { "SvStrictSpectateMode", new ConfigInt("SvStrictSpectateMode", "sv_strict_spectate_mode", 0, 0, 1, CFGFLAG_SERVER, "Restricts information in spectator mode") },
            { "SvVoteSpectate", new ConfigInt("SvVoteSpectate", "sv_vote_spectate", 1, 0, 1, CFGFLAG_SERVER, "Allow voting to move players to spectators") },
            { "SvVoteSpectateRejoindelay", new ConfigInt("SvVoteSpectateRejoindelay", "sv_vote_spectate_rejoindelay", 3, 0, 1000, CFGFLAG_SERVER, "How many minutes to wait before a player can rejoin after being moved to spectators by vote") },
            { "SvVoteKick", new ConfigInt("SvVoteKick", "sv_vote_kick", 1, 0, 1, CFGFLAG_SERVER, "Allow voting to kick players") },
            { "SvVoteKickMin", new ConfigInt("SvVoteKickMin", "sv_vote_kick_min", 0, 0, (int)Consts.MAX_CLIENTS, CFGFLAG_SERVER, "Minimum number of players required to start a kick vote") },
            { "SvVoteKickBantime", new ConfigInt("SvVoteKickBantime", "sv_vote_kick_bantime", 5, 0, 1440, CFGFLAG_SERVER, "The time to ban a player if kicked by vote. 0 makes it just use kick") },

            { "SvMapUpdateRate", new ConfigInt("SvMapUpdateRate", "sv_mapupdaterate", 5, 1, 100, CFGFLAG_SERVER, "(Tw32) real id <-> vanilla id players map update rate") },

            { "DbgFocus", new ConfigInt("DbgFocus", "dbg_focus", 0, 0, 1, CFGFLAG_CLIENT, "") },
            { "DbgTuning", new ConfigInt("DbgTuning", "dbg_tuning", 0, 0, 1, CFGFLAG_CLIENT, "") },
        };
    }
}
