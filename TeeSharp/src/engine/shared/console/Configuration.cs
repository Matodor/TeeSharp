using System;
using System.Collections.Generic;
using System.Linq;

namespace TeeSharp
{
    public abstract class ConfigVariable
    {
        public string Name;
        public string ConsoleCommand;
        public Configuration.ConfigFlags Flags;
        public string Description;
    }

    public class ConfigInt : ConfigVariable
    {
        public int Default;
        public int Min;
        public int Max;

        public ConfigInt(string name, string consoleCommand, int def, int min, int max, 
            Configuration.ConfigFlags flags, string desc)
        {
            Name = name;
            ConsoleCommand = consoleCommand;
            Default = def;
            Min = min;
            Max = max;
            Flags = flags;
            Description = desc;
        }
    }

    public class ConfigStr : ConfigVariable
    {
        public int MaxLength;
        public string Default;

        public ConfigStr(string name, string consoleCommand, int maxLength, string def, 
            Configuration.ConfigFlags flags, string desc)
        {
            Name = name;
            ConsoleCommand = consoleCommand;
            MaxLength = maxLength;
            Default = def;
            Flags = flags;
            Description = desc;
        }
    }

    public partial class Configuration
    {
        [Flags]
        public enum ConfigFlags
        {
            SAVE = 1 << 0,
            CLIENT = 1 << 1,
            SERVER = 1 << 2,
            STORE = 1 << 3,
            MASTER = 1 << 4,
            ECON = 1 << 5,
        }

        public IReadOnlyList<KeyValuePair<string, object>> Variables => _variablesDictionary.ToList().AsReadOnly();

        private readonly Dictionary<string, object> _variablesDictionary = new Dictionary<string, object>();

        public Configuration()
        {
            foreach (var o in _configVariablesDictionary)
                _variablesDictionary.Add(o.Key, o.Value);
            foreach (var o in _default_variablesDictionary)
                _variablesDictionary.Add(o.Key, o.Value);
        }

        public virtual int GetInt(string name)
        {
            if (_variablesDictionary.ContainsKey(name))
                return ((ConfigInt)_variablesDictionary[name]).Default;
            return 0;
        }

        public virtual string GetString(string name)
        {
            if (_variablesDictionary.ContainsKey(name))
                return ((ConfigStr)_variablesDictionary[name]).Default;
            return "";
        }

        public virtual void SetInt(string name, int value)
        {
            if (_variablesDictionary.ContainsKey(name))
            {
                var c = (ConfigInt)_variablesDictionary[name];
                c.Default = Math.Clamp(value, c.Min, c.Max);
            }
        }

        public virtual void SetString(string name, string value)
        {
            if (_variablesDictionary.ContainsKey(name))
                ((ConfigStr)_variablesDictionary[name]).Default = value;
        }

        private readonly Dictionary<string, object> _configVariablesDictionary = new Dictionary<string, object>
        {
            { "ConnTimeout", new ConfigInt("ConnTimeout", "conn_timeout", 100, 5, 1000, ConfigFlags.SAVE | ConfigFlags.CLIENT | ConfigFlags.SERVER, "Network timeout") },
            { "ConnTimeoutProtection", new ConfigInt("ConnTimeoutProtection", "conn_timeout_protection", 1000, 5, 10000, ConfigFlags.SAVE | ConfigFlags.SERVER, "Network timeout protection") },
            { "SvSpoofProtection", new ConfigInt("SvSpoofProtection", "sv_spoof_protection", 0, 0, 1, ConfigFlags.SERVER, "Enable spoof protection") },
            { "SvMapWindow", new ConfigInt("SvMapWindow", "sv_map_window", 15, 0, 100, ConfigFlags.SERVER, "Map downloading send-ahead window") },
            { "SvFastDownload", new ConfigInt("SvFastDownload", "sv_fast_download", 1, 0, 1, ConfigFlags.SERVER, "Enables fast download of maps") },
            { "SvNetlimit", new ConfigInt("SvNetlimit", "sv_netlimit", 0, 0, 10000, ConfigFlags.SERVER, "Netlimit: Maximum amount of traffic a client is allowed to use (in kb/s)") },
            { "SvNetlimitAlpha", new ConfigInt("SvNetlimitAlpha", "sv_netlimit_alpha", 50, 1, 100, ConfigFlags.SERVER, "Netlimit: Alpha of Exponention moving average") },

            { "PlayerName", new ConfigStr("PlayerName", "player_name", 16, "nameless tee", ConfigFlags.SAVE|ConfigFlags.CLIENT, "Name of the player") },
            { "PlayerClan", new ConfigStr("PlayerClan", "player_clan", 12, "", ConfigFlags.SAVE|ConfigFlags.CLIENT, "Clan of the player") },
            { "PlayerCountry", new ConfigInt("PlayerCountry", "player_country", -1, -1, 1000, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Country of the player") },
            { "Password", new ConfigStr("Password", "password", 32, "", ConfigFlags.CLIENT|ConfigFlags.SERVER, "Password to the server") },
            { "Logfile", new ConfigStr("Logfile", "logfile", 128, "", ConfigFlags.SAVE|ConfigFlags.CLIENT|ConfigFlags.SERVER, "Filename to log all output to") },
            { "ConsoleOutputLevel", new ConfigInt("ConsoleOutputLevel", "console_output_level", 0, 0, 2, ConfigFlags.CLIENT|ConfigFlags.SERVER, "Adjusts the amount of information in the console") },

            { "ClCpuThrottle", new ConfigInt("ClCpuThrottle", "cl_cpu_throttle", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "") },
            { "ClEditor", new ConfigInt("ClEditor", "cl_editor", 0, 0, 1, ConfigFlags.CLIENT, "") },
            { "ClLoadCountryFlags", new ConfigInt("ClLoadCountryFlags", "cl_load_country_flags", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Load and show country flags") },

            { "ClAutoDemoRecord", new ConfigInt("ClAutoDemoRecord", "cl_auto_demo_record", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Automatically record demos") },
            { "ClAutoDemoMax", new ConfigInt("ClAutoDemoMax", "cl_auto_demo_max", 10, 0, 1000, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Maximum number of automatically recorded demos (0 = no limit)") },
            { "ClAutoScreenshot", new ConfigInt("ClAutoScreenshot", "cl_auto_screenshot", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Automatically take game over screenshot") },
            { "ClAutoScreenshotMax", new ConfigInt("ClAutoScreenshotMax", "cl_auto_screenshot_max", 10, 0, 1000, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Maximum number of automatically created screenshots (0 = no limit)") },

            { "ClEventthread", new ConfigInt("ClEventthread", "cl_eventthread", 0, 0, 1, ConfigFlags.CLIENT, "Enables the usage of a thread to pump the events") },

            { "InpGrab", new ConfigInt("InpGrab", "inp_grab", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Use forceful input grabbing method") },

            { "BrFilterString", new ConfigStr("BrFilterString", "br_filter_string", 25, "", ConfigFlags.SAVE|ConfigFlags.CLIENT, "Server browser filtering string") },
            { "BrFilterFull", new ConfigInt("BrFilterFull", "br_filter_full", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Filter out full server in browser") },
            { "BrFilterEmpty", new ConfigInt("BrFilterEmpty", "br_filter_empty", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Filter out empty server in browser") },
            { "BrFilterSpectators", new ConfigInt("BrFilterSpectators", "br_filter_spectators", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Filter out spectators from player numbers") },
            { "BrFilterFriends", new ConfigInt("BrFilterFriends", "br_filter_friends", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Filter out servers with no friends") },
            { "BrFilterCountry", new ConfigInt("BrFilterCountry", "br_filter_country", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Filter out servers with non-matching player country") },
            { "BrFilterCountryIndex", new ConfigInt("BrFilterCountryIndex", "br_filter_country_index", -1, -1, 999, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Player country to filter by in the server browser") },
            { "BrFilterPw", new ConfigInt("BrFilterPw", "br_filter_pw", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Filter out password protected servers in browser") },
            { "BrFilterPing", new ConfigInt("BrFilterPing", "br_filter_ping", 999, 0, 999, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Ping to filter by in the server browser") },
            { "BrFilterGametype", new ConfigStr("BrFilterGametype", "br_filter_gametype", 128, "", ConfigFlags.SAVE|ConfigFlags.CLIENT, "Game types to filter") },
            { "BrFilterGametypeStrict", new ConfigInt("BrFilterGametypeStrict", "br_filter_gametype_strict", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Strict gametype filter") },
            { "BrFilterServerAddress", new ConfigStr("BrFilterServerAddress", "br_filter_serveraddress", 128, "", ConfigFlags.SAVE|ConfigFlags.CLIENT, "Server address to filter") },
            { "BrFilterPure", new ConfigInt("BrFilterPure", "br_filter_pure", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Filter out non-standard servers in browser") },
            { "BrFilterPureMap", new ConfigInt("BrFilterPureMap", "br_filter_pure_map", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Filter out non-standard maps in browser") },
            { "BrFilterCompatversion", new ConfigInt("BrFilterCompatversion", "br_filter_compatversion", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Filter out non-compatible servers in browser") },

            { "BrSort", new ConfigInt("BrSort", "br_sort", 0, 0, 256, ConfigFlags.SAVE|ConfigFlags.CLIENT, "") },
            { "BrSortOrder", new ConfigInt("BrSortOrder", "br_sort_order", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "") },
            { "BrMaxRequests", new ConfigInt("BrMaxRequests", "br_max_requests", 25, 0, 1000, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Number of requests to use when refreshing server browser") },

            { "SndBufferSize", new ConfigInt("SndBufferSize", "snd_buffer_size", 512, 128, 32768, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Sound buffer size") },
            { "SndRate", new ConfigInt("SndRate", "snd_rate", 48000, 0, 0, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Sound mixing rate") },
            { "SndEnable", new ConfigInt("SndEnable", "snd_enable", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Sound enable") },
            { "SndMusic", new ConfigInt("SndMusic", "snd_enable_music", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Play background music") },
            { "SndVolume", new ConfigInt("SndVolume", "snd_volume", 100, 0, 100, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Sound volume") },
            { "SndDevice", new ConfigInt("SndDevice", "snd_device", -1, 0, 0, ConfigFlags.SAVE|ConfigFlags.CLIENT, "(deprecated) Sound device to use") },

            { "SndNonactiveMute", new ConfigInt("SndNonactiveMute", "snd_nonactive_mute", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "") },

            { "GfxScreenWidth", new ConfigInt("GfxScreenWidth", "gfx_screen_width", 800, 0, 0, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Screen resolution width") },
            { "GfxScreenHeight", new ConfigInt("GfxScreenHeight", "gfx_screen_height", 600, 0, 0, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Screen resolution height") },
            { "GfxFullscreen", new ConfigInt("GfxFullscreen", "gfx_fullscreen", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Fullscreen") },
            { "GfxAlphabits", new ConfigInt("GfxAlphabits", "gfx_alphabits", 0, 0, 0, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Alpha bits for framebuffer (fullscreen only)") },
            { "GfxColorDepth", new ConfigInt("GfxColorDepth", "gfx_color_depth", 24, 16, 24, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Colors bits for framebuffer (fullscreen only)") },
            { "GfxClear", new ConfigInt("GfxClear", "gfx_clear", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Clear screen before rendering") },
            { "GfxVsync", new ConfigInt("GfxVsync", "gfx_vsync", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Vertical sync") },
            { "GfxDisplayAllModes", new ConfigInt("GfxDisplayAllModes", "gfx_display_all_modes", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "") },
            { "GfxTextureCompression", new ConfigInt("GfxTextureCompression", "gfx_texture_compression", 0, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Use texture compression") },
            { "GfxHighDetail", new ConfigInt("GfxHighDetail", "gfx_high_detail", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "High detail") },
            { "GfxTextureQuality", new ConfigInt("GfxTextureQuality", "gfx_texture_quality", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "") },
            { "GfxFsaaSamples", new ConfigInt("GfxFsaaSamples", "gfx_fsaa_samples", 0, 0, 16, ConfigFlags.SAVE|ConfigFlags.CLIENT, "FSAA Samples") },
            { "GfxRefreshRate", new ConfigInt("GfxRefreshRate", "gfx_refresh_rate", 0, 0, 0, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Screen refresh rate") },
            { "GfxFinish", new ConfigInt("GfxFinish", "gfx_finish", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "") },
            { "GfxAsyncRender", new ConfigInt("GfxAsyncRender", "gfx_asyncrender", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Do rendering async from the the update") },

            { "GfxThreaded", new ConfigInt("GfxThreaded", "gfx_threaded", 1, 0, 1, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Use the threaded graphics backend") },

            { "InpMousesens", new ConfigInt("InpMousesens", "inp_mousesens", 100, 5, 100000, ConfigFlags.SAVE|ConfigFlags.CLIENT, "Mouse sensitivity") },

            { "SvName", new ConfigStr("SvName", "sv_name", 128, "TeeSharp server", ConfigFlags.SERVER, "Server name") },
            { "Bindaddr", new ConfigStr("Bindaddr", "bindaddr", 128, "", ConfigFlags.CLIENT|ConfigFlags.SERVER|ConfigFlags.MASTER, "Address to bind the client/server to") },
            { "SvPort", new ConfigInt("SvPort", "sv_port", 8303, 0, 65535, ConfigFlags.SERVER, "Port to use for the server") },
            { "SvExternalPort", new ConfigInt("SvExternalPort", "sv_external_port", 0, 0, 0, ConfigFlags.SERVER, "External port to report to the master servers") },
            { "SvMap", new ConfigStr("SvMap", "sv_map", 128, "dm1", ConfigFlags.SERVER, "Map to use on the server") },
            { "SvMaxClients", new ConfigInt("SvMaxClients", "sv_max_clients", 64, 1, Consts.MAX_CLIENTS, ConfigFlags.SERVER, "Maximum number of clients that are allowed on a server") },
            { "SvMaxClientsPerIP", new ConfigInt("SvMaxClientsPerIP", "sv_max_clients_per_ip", 2, 1, Consts.MAX_CLIENTS, ConfigFlags.SERVER, "Maximum number of clients with the same IP that can connect to the server") },
            { "SvHighBandwidth", new ConfigInt("SvHighBandwidth", "sv_high_bandwidth", 0, 0, 1, ConfigFlags.SERVER, "Use high bandwidth mode. Doubles the bandwidth required for the server. LAN use only") },
            { "SvRegister", new ConfigInt("SvRegister", "sv_register", 1, 0, 1, ConfigFlags.SERVER, "Register server with master server for public listing") },
            { "SvRconPassword", new ConfigStr("SvRconPassword", "sv_rcon_password", 32, "", ConfigFlags.SERVER, "Remote console password (full access)") },
            { "SvRconModPassword", new ConfigStr("SvRconModPassword", "sv_rcon_mod_password", 32, "", ConfigFlags.SERVER, "Remote console password for moderators (limited access)") },
            { "SvRconMaxTries", new ConfigInt("SvRconMaxTries", "sv_rcon_max_tries", 5, 0, 100, ConfigFlags.SERVER, "Maximum number of tries for remote console authentication") },
            { "SvRconBantime", new ConfigInt("SvRconBantime", "sv_rcon_bantime", 0, 0, 1440, ConfigFlags.SERVER, "The time a client gets banned if remote console authentication fails. 0 makes it just use kick") },
            { "SvAutoDemoRecord", new ConfigInt("SvAutoDemoRecord", "sv_auto_demo_record", 0, 0, 1, ConfigFlags.SERVER, "Automatically record demos") },
            { "SvAutoDemoMax", new ConfigInt("SvAutoDemoMax", "sv_auto_demo_max", 10, 0, 1000, ConfigFlags.SERVER, "Maximum number of automatically recorded demos (0 = no limit)") },

            { "EcBindaddr", new ConfigStr("EcBindaddr", "ec_bindaddr", 128, "localhost", ConfigFlags.ECON, "Address to bind the external console to. Anything but 'localhost' is dangerous") },
            { "EcPort", new ConfigInt("EcPort", "ec_port", 0, 0, 0, ConfigFlags.ECON, "Port to use for the external console") },
            { "EcPassword", new ConfigStr("EcPassword", "ec_password", 32, "", ConfigFlags.ECON, "External console password") },
            { "EcBantime", new ConfigInt("EcBantime", "ec_bantime", 0, 0, 1440, ConfigFlags.ECON, "The time a client gets banned if econ authentication fails. 0 just closes the connection") },
            { "EcAuthTimeout", new ConfigInt("EcAuthTimeout", "ec_auth_timeout", 30, 1, 120, ConfigFlags.ECON, "Time in seconds before the the econ authentification times out ") },
            { "EcOutputLevel", new ConfigInt("EcOutputLevel", "ec_output_level", 1, 0, 2, ConfigFlags.ECON, "Adjusts the amount of information in the external console") },

            { "Debug", new ConfigInt("Debug", "debug", 0, 0, 1, ConfigFlags.CLIENT|ConfigFlags.SERVER, "Debug mode") },
            { "DbgStress", new ConfigInt("DbgStress", "dbg_stress", 0, 0, 0, ConfigFlags.CLIENT|ConfigFlags.SERVER, "Stress systems") },
            { "DbgStressNetwork", new ConfigInt("DbgStressNetwork", "dbg_stress_network", 0, 0, 0, ConfigFlags.CLIENT|ConfigFlags.SERVER, "Stress network") },
            { "DbgPref", new ConfigInt("DbgPref", "dbg_pref", 0, 0, 1, ConfigFlags.SERVER, "Performance outputs") },
            { "DbgGraphs", new ConfigInt("DbgGraphs", "dbg_graphs", 0, 0, 1, ConfigFlags.CLIENT, "Performance graphs") },
            { "DbgHitch", new ConfigInt("DbgHitch", "dbg_hitch", 0, 0, 0, ConfigFlags.SERVER, "Hitch warnings") },
            { "DbgStressServer", new ConfigStr("DbgStressServer", "dbg_stress_server", 32, "localhost", ConfigFlags.CLIENT, "Server to stress") },
            { "DbgResizable", new ConfigInt("DbgResizable", "dbg_resizable", 0, 0, 0, ConfigFlags.CLIENT, "Enables window resizing") },
        };
    }
}
