using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public partial class CConfiguration
    {
        public static CConfiguration Instance { get { return instance; } }
        private static CConfiguration instance = new CConfiguration();

        public const int
            CFGFLAG_SAVE = 1,
            CFGFLAG_CLIENT = 2,
            CFGFLAG_SERVER = 4,
            CFGFLAG_STORE = 8,
            CFGFLAG_MASTER = 16,
            CFGFLAG_ECON = 32;

        public static Dictionary<string, object> Variables { get { return variablesDictionary; } }

        public int GetInt(string name)
        {
            if (variablesDictionary.ContainsKey(name))
                return ((ConfigInt)variablesDictionary[name]).Default;
            return 0;
        }

        public string GetString(string name)
        {
            if (variablesDictionary.ContainsKey(name))
                return ((ConfigStr)variablesDictionary[name]).Default;
            return "";
        }

        public void SetInt(string name, int value)
        {
            if (variablesDictionary.ContainsKey(name))
            {
                var c = (ConfigInt) variablesDictionary[name];
                c.Default = CMath.clamp(value, c.Min, c.Max);
            }
        }

        public void SetString(string name, string value)
        {
            if (variablesDictionary.ContainsKey(name))
                ((ConfigStr) variablesDictionary[name]).Default = value;
        }

        static CConfiguration()
        {
            foreach (var o in default_variablesDictionary)
                variablesDictionary.Add(o.Key, o.Value);
            foreach (var o in default_configVariablesDictionary)
                variablesDictionary.Add(o.Key, o.Value);
        }

        private static readonly Dictionary<string, object> variablesDictionary = new Dictionary<string, object>();
        private static readonly Dictionary<string, object> default_configVariablesDictionary = new Dictionary<string, object>
        {
            { "ConnTimeout", new ConfigInt("ConnTimeout", "conn_timeout", 100, 5, 1000, CFGFLAG_SAVE | CFGFLAG_CLIENT | CFGFLAG_SERVER, "Network timeout") },
            { "ConnTimeoutProtection", new ConfigInt("ConnTimeoutProtection", "conn_timeout_protection", 1000, 5, 10000, CFGFLAG_SAVE | CFGFLAG_SERVER, "Network timeout protection") },
            { "SvSpoofProtection", new ConfigInt("SvSpoofProtection", "sv_spoof_protection", 0, 0, 1, CFGFLAG_SERVER, "Enable spoof protection") },
            { "SvMapWindow", new ConfigInt("SvMapWindow", "sv_map_window", 15, 0, 100, CFGFLAG_SERVER, "Map downloading send-ahead window") },
            { "SvFastDownload", new ConfigInt("SvFastDownload", "sv_fast_download", 1, 0, 1, CFGFLAG_SERVER, "Enables fast download of maps") },
            { "SvNetlimit", new ConfigInt("SvNetlimit", "sv_netlimit", 0, 0, 10000, CFGFLAG_SERVER, "Netlimit: Maximum amount of traffic a client is allowed to use (in kb/s)") },
            { "SvNetlimitAlpha", new ConfigInt("SvNetlimitAlpha", "sv_netlimit_alpha", 50, 1, 100, CFGFLAG_SERVER, "Netlimit: Alpha of Exponention moving average") },

            { "PlayerName", new ConfigStr("PlayerName", "player_name", 16, "nameless tee", CFGFLAG_SAVE|CFGFLAG_CLIENT, "Name of the player") },
            { "PlayerClan", new ConfigStr("PlayerClan", "player_clan", 12, "", CFGFLAG_SAVE|CFGFLAG_CLIENT, "Clan of the player") },
            { "PlayerCountry", new ConfigInt("PlayerCountry", "player_country", -1, -1, 1000, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Country of the player") },
            { "Password", new ConfigStr("Password", "password", 32, "", CFGFLAG_CLIENT|CFGFLAG_SERVER, "Password to the server") },
            { "Logfile", new ConfigStr("Logfile", "logfile", 128, "", CFGFLAG_SAVE|CFGFLAG_CLIENT|CFGFLAG_SERVER, "Filename to log all output to") },
            { "ConsoleOutputLevel", new ConfigInt("ConsoleOutputLevel", "console_output_level", 0, 0, 2, CFGFLAG_CLIENT|CFGFLAG_SERVER, "Adjusts the amount of information in the console") },

            { "ClCpuThrottle", new ConfigInt("ClCpuThrottle", "cl_cpu_throttle", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "") },
            { "ClEditor", new ConfigInt("ClEditor", "cl_editor", 0, 0, 1, CFGFLAG_CLIENT, "") },
            { "ClLoadCountryFlags", new ConfigInt("ClLoadCountryFlags", "cl_load_country_flags", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Load and show country flags") },

            { "ClAutoDemoRecord", new ConfigInt("ClAutoDemoRecord", "cl_auto_demo_record", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Automatically record demos") },
            { "ClAutoDemoMax", new ConfigInt("ClAutoDemoMax", "cl_auto_demo_max", 10, 0, 1000, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Maximum number of automatically recorded demos (0 = no limit)") },
            { "ClAutoScreenshot", new ConfigInt("ClAutoScreenshot", "cl_auto_screenshot", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Automatically take game over screenshot") },
            { "ClAutoScreenshotMax", new ConfigInt("ClAutoScreenshotMax", "cl_auto_screenshot_max", 10, 0, 1000, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Maximum number of automatically created screenshots (0 = no limit)") },

            { "ClEventthread", new ConfigInt("ClEventthread", "cl_eventthread", 0, 0, 1, CFGFLAG_CLIENT, "Enables the usage of a thread to pump the events") },

            { "InpGrab", new ConfigInt("InpGrab", "inp_grab", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Use forceful input grabbing method") },

            { "BrFilterString", new ConfigStr("BrFilterString", "br_filter_string", 25, "", CFGFLAG_SAVE|CFGFLAG_CLIENT, "Server browser filtering string") },
            { "BrFilterFull", new ConfigInt("BrFilterFull", "br_filter_full", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Filter out full server in browser") },
            { "BrFilterEmpty", new ConfigInt("BrFilterEmpty", "br_filter_empty", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Filter out empty server in browser") },
            { "BrFilterSpectators", new ConfigInt("BrFilterSpectators", "br_filter_spectators", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Filter out spectators from player numbers") },
            { "BrFilterFriends", new ConfigInt("BrFilterFriends", "br_filter_friends", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Filter out servers with no friends") },
            { "BrFilterCountry", new ConfigInt("BrFilterCountry", "br_filter_country", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Filter out servers with non-matching player country") },
            { "BrFilterCountryIndex", new ConfigInt("BrFilterCountryIndex", "br_filter_country_index", -1, -1, 999, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Player country to filter by in the server browser") },
            { "BrFilterPw", new ConfigInt("BrFilterPw", "br_filter_pw", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Filter out password protected servers in browser") },
            { "BrFilterPing", new ConfigInt("BrFilterPing", "br_filter_ping", 999, 0, 999, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Ping to filter by in the server browser") },
            { "BrFilterGametype", new ConfigStr("BrFilterGametype", "br_filter_gametype", 128, "", CFGFLAG_SAVE|CFGFLAG_CLIENT, "Game types to filter") },
            { "BrFilterGametypeStrict", new ConfigInt("BrFilterGametypeStrict", "br_filter_gametype_strict", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Strict gametype filter") },
            { "BrFilterServerAddress", new ConfigStr("BrFilterServerAddress", "br_filter_serveraddress", 128, "", CFGFLAG_SAVE|CFGFLAG_CLIENT, "Server address to filter") },
            { "BrFilterPure", new ConfigInt("BrFilterPure", "br_filter_pure", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Filter out non-standard servers in browser") },
            { "BrFilterPureMap", new ConfigInt("BrFilterPureMap", "br_filter_pure_map", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Filter out non-standard maps in browser") },
            { "BrFilterCompatversion", new ConfigInt("BrFilterCompatversion", "br_filter_compatversion", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Filter out non-compatible servers in browser") },

            { "BrSort", new ConfigInt("BrSort", "br_sort", 0, 0, 256, CFGFLAG_SAVE|CFGFLAG_CLIENT, "") },
            { "BrSortOrder", new ConfigInt("BrSortOrder", "br_sort_order", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "") },
            { "BrMaxRequests", new ConfigInt("BrMaxRequests", "br_max_requests", 25, 0, 1000, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Number of requests to use when refreshing server browser") },

            { "SndBufferSize", new ConfigInt("SndBufferSize", "snd_buffer_size", 512, 128, 32768, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Sound buffer size") },
            { "SndRate", new ConfigInt("SndRate", "snd_rate", 48000, 0, 0, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Sound mixing rate") },
            { "SndEnable", new ConfigInt("SndEnable", "snd_enable", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Sound enable") },
            { "SndMusic", new ConfigInt("SndMusic", "snd_enable_music", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Play background music") },
            { "SndVolume", new ConfigInt("SndVolume", "snd_volume", 100, 0, 100, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Sound volume") },
            { "SndDevice", new ConfigInt("SndDevice", "snd_device", -1, 0, 0, CFGFLAG_SAVE|CFGFLAG_CLIENT, "(deprecated) Sound device to use") },

            { "SndNonactiveMute", new ConfigInt("SndNonactiveMute", "snd_nonactive_mute", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "") },

            { "GfxScreenWidth", new ConfigInt("GfxScreenWidth", "gfx_screen_width", 800, 0, 0, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Screen resolution width") },
            { "GfxScreenHeight", new ConfigInt("GfxScreenHeight", "gfx_screen_height", 600, 0, 0, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Screen resolution height") },
            { "GfxFullscreen", new ConfigInt("GfxFullscreen", "gfx_fullscreen", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Fullscreen") },
            { "GfxAlphabits", new ConfigInt("GfxAlphabits", "gfx_alphabits", 0, 0, 0, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Alpha bits for framebuffer (fullscreen only)") },
            { "GfxColorDepth", new ConfigInt("GfxColorDepth", "gfx_color_depth", 24, 16, 24, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Colors bits for framebuffer (fullscreen only)") },
            { "GfxClear", new ConfigInt("GfxClear", "gfx_clear", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Clear screen before rendering") },
            { "GfxVsync", new ConfigInt("GfxVsync", "gfx_vsync", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Vertical sync") },
            { "GfxDisplayAllModes", new ConfigInt("GfxDisplayAllModes", "gfx_display_all_modes", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "") },
            { "GfxTextureCompression", new ConfigInt("GfxTextureCompression", "gfx_texture_compression", 0, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Use texture compression") },
            { "GfxHighDetail", new ConfigInt("GfxHighDetail", "gfx_high_detail", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "High detail") },
            { "GfxTextureQuality", new ConfigInt("GfxTextureQuality", "gfx_texture_quality", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "") },
            { "GfxFsaaSamples", new ConfigInt("GfxFsaaSamples", "gfx_fsaa_samples", 0, 0, 16, CFGFLAG_SAVE|CFGFLAG_CLIENT, "FSAA Samples") },
            { "GfxRefreshRate", new ConfigInt("GfxRefreshRate", "gfx_refresh_rate", 0, 0, 0, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Screen refresh rate") },
            { "GfxFinish", new ConfigInt("GfxFinish", "gfx_finish", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "") },
            { "GfxAsyncRender", new ConfigInt("GfxAsyncRender", "gfx_asyncrender", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Do rendering async from the the update") },

            { "GfxThreaded", new ConfigInt("GfxThreaded", "gfx_threaded", 1, 0, 1, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Use the threaded graphics backend") },

            { "InpMousesens", new ConfigInt("InpMousesens", "inp_mousesens", 100, 5, 100000, CFGFLAG_SAVE|CFGFLAG_CLIENT, "Mouse sensitivity") },

            { "SvName", new ConfigStr("SvName", "sv_name", 128, "[mLife] Role play server (city)", CFGFLAG_SERVER, "Server name") },
            { "Bindaddr", new ConfigStr("Bindaddr", "bindaddr", 128, "", CFGFLAG_CLIENT|CFGFLAG_SERVER|CFGFLAG_MASTER, "Address to bind the client/server to") },
            { "SvPort", new ConfigInt("SvPort", "sv_port", 8303, 0, 65535, CFGFLAG_SERVER, "Port to use for the server") },
            { "SvExternalPort", new ConfigInt("SvExternalPort", "sv_external_port", 0, 0, 0, CFGFLAG_SERVER, "External port to report to the master servers") },
            { "SvMap", new ConfigStr("SvMap", "sv_map", 128, "dm1", CFGFLAG_SERVER, "Map to use on the server") },
            { "SvMaxClients", new ConfigInt("SvMaxClients", "sv_max_clients", 64, 1, (int)Consts.MAX_CLIENTS, CFGFLAG_SERVER, "Maximum number of clients that are allowed on a server") },
            { "SvMaxClientsPerIP", new ConfigInt("SvMaxClientsPerIP", "sv_max_clients_per_ip", 2, 1, (int)Consts.MAX_CLIENTS, CFGFLAG_SERVER, "Maximum number of clients with the same IP that can connect to the server") },
            { "SvHighBandwidth", new ConfigInt("SvHighBandwidth", "sv_high_bandwidth", 0, 0, 1, CFGFLAG_SERVER, "Use high bandwidth mode. Doubles the bandwidth required for the server. LAN use only") },
            { "SvRegister", new ConfigInt("SvRegister", "sv_register", 1, 0, 1, CFGFLAG_SERVER, "Register server with master server for public listing") },
            { "SvRconPassword", new ConfigStr("SvRconPassword", "sv_rcon_password", 32, "", CFGFLAG_SERVER, "Remote console password (full access)") },
            { "SvRconModPassword", new ConfigStr("SvRconModPassword", "sv_rcon_mod_password", 32, "", CFGFLAG_SERVER, "Remote console password for moderators (limited access)") },
            { "SvRconMaxTries", new ConfigInt("SvRconMaxTries", "sv_rcon_max_tries", 5, 0, 100, CFGFLAG_SERVER, "Maximum number of tries for remote console authentication") },
            { "SvRconBantime", new ConfigInt("SvRconBantime", "sv_rcon_bantime", 0, 0, 1440, CFGFLAG_SERVER, "The time a client gets banned if remote console authentication fails. 0 makes it just use kick") },
            { "SvAutoDemoRecord", new ConfigInt("SvAutoDemoRecord", "sv_auto_demo_record", 0, 0, 1, CFGFLAG_SERVER, "Automatically record demos") },
            { "SvAutoDemoMax", new ConfigInt("SvAutoDemoMax", "sv_auto_demo_max", 10, 0, 1000, CFGFLAG_SERVER, "Maximum number of automatically recorded demos (0 = no limit)") },

            { "EcBindaddr", new ConfigStr("EcBindaddr", "ec_bindaddr", 128, "localhost", CFGFLAG_ECON, "Address to bind the external console to. Anything but 'localhost' is dangerous") },
            { "EcPort", new ConfigInt("EcPort", "ec_port", 0, 0, 0, CFGFLAG_ECON, "Port to use for the external console") },
            { "EcPassword", new ConfigStr("EcPassword", "ec_password", 32, "", CFGFLAG_ECON, "External console password") },
            { "EcBantime", new ConfigInt("EcBantime", "ec_bantime", 0, 0, 1440, CFGFLAG_ECON, "The time a client gets banned if econ authentication fails. 0 just closes the connection") },
            { "EcAuthTimeout", new ConfigInt("EcAuthTimeout", "ec_auth_timeout", 30, 1, 120, CFGFLAG_ECON, "Time in seconds before the the econ authentification times out ") },
            { "EcOutputLevel", new ConfigInt("EcOutputLevel", "ec_output_level", 1, 0, 2, CFGFLAG_ECON, "Adjusts the amount of information in the external console") },

            { "Debug", new ConfigInt("Debug", "debug", 0, 0, 1, CFGFLAG_CLIENT|CFGFLAG_SERVER, "Debug mode") },
            { "DbgStress", new ConfigInt("DbgStress", "dbg_stress", 0, 0, 0, CFGFLAG_CLIENT|CFGFLAG_SERVER, "Stress systems") },
            { "DbgStressNetwork", new ConfigInt("DbgStressNetwork", "dbg_stress_network", 0, 0, 0, CFGFLAG_CLIENT|CFGFLAG_SERVER, "Stress network") },
            { "DbgPref", new ConfigInt("DbgPref", "dbg_pref", 0, 0, 1, CFGFLAG_SERVER, "Performance outputs") },
            { "DbgGraphs", new ConfigInt("DbgGraphs", "dbg_graphs", 0, 0, 1, CFGFLAG_CLIENT, "Performance graphs") },
            { "DbgHitch", new ConfigInt("DbgHitch", "dbg_hitch", 0, 0, 0, CFGFLAG_SERVER, "Hitch warnings") },
            { "DbgStressServer", new ConfigStr("DbgStressServer", "dbg_stress_server", 32, "localhost", CFGFLAG_CLIENT, "Server to stress") },
            { "DbgResizable", new ConfigInt("DbgResizable", "dbg_resizable", 0, 0, 0, CFGFLAG_CLIENT, "Enables window resizing") },
        };
    }

    public class ConfigInt
    {
        public string Name { get; set; }
        public string ScriptName { get; set; }
        public int Default { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Flags { get; set; }
        public string Desc { get; set; }

        public ConfigInt(string name, string scriptName, int def, int min, int max, int flags, string desc)
        {
            Name = name;
            ScriptName = scriptName;
            Default = def;
            Min = min;
            Max = max;
            Flags = flags;
            Desc = desc;
        }
    }

    public class ConfigStr
    {
        public string Name { get; set; }
        public string ScriptName { get; set; }
        public int MaxLen { get; set; }
        public string Default { get; set; }
        public int Flags { get; set; }
        public string Desc { get; set; }

        public ConfigStr(string name, string scriptName, int len, string def, int flags, string desc)
        {
            Name = name;
            ScriptName = scriptName;
            MaxLen = len;
            Default = def;
            Flags = flags;
            Desc = desc;
        }
    }
}
