using System.Collections.Generic;
using TeeSharp.Core;

namespace TeeSharp.Common.Config
{
    public class Config : BaseConfig
    {
        public Config()
        {
            Variables = new Dictionary<string, ConfigVariable>();
        }

        public override void Init(ConfigFlags saveMask)
        {
            SaveMask = saveMask;

            AppendVariables(new Dictionary<string, ConfigVariable>()
            {
                { "ConnTimeout", new ConfigInt("conn_timeout", 100, 5, 1000, ConfigFlags.Save | ConfigFlags.Client | ConfigFlags.Server, "Network timeout") },
                { "ConnTimeoutProtection", new ConfigInt("conn_timeout_protection", 1000, 5, 10000, ConfigFlags.Save | ConfigFlags.Client | ConfigFlags.Server, "Network timeout protection") },
                { "Password", new ConfigString("password", 32, "", ConfigFlags.Client|ConfigFlags.Server, "Password to the server") },
                { "Logfile", new ConfigString("logfile", 128, "", ConfigFlags.Save|ConfigFlags.Client|ConfigFlags.Server, "Filename to log all output to") },
                { "ConsoleOutputLevel", new ConfigInt("console_output_level", 0, 0, 2, ConfigFlags.Client|ConfigFlags.Server, "Adjusts the amount of information in the console") },
                { "Bindaddr", new ConfigString("bindaddr", 128, "", ConfigFlags.Client|ConfigFlags.Server|ConfigFlags.Master, "Address to bind the client/server to") },

                { "EcBindaddr", new ConfigString("ec_bindaddr", 128, "localhost", ConfigFlags.Econ, "Address to bind the external console to. Anything but 'localhost' is dangerous") },
                { "EcPort", new ConfigInt("ec_port", 0, 0, 0, ConfigFlags.Econ, "Port to use for the external console") },
                { "EcPassword", new ConfigString("ec_password", 32, "", ConfigFlags.Econ, "External console password") },
                { "EcBantime", new ConfigInt("ec_bantime", 0, 0, 1440, ConfigFlags.Econ, "The time a client gets banned if econ authentication fails. 0 just closes the connection") },
                { "EcAuthTimeout", new ConfigInt("ec_auth_timeout", 30, 1, 120, ConfigFlags.Econ, "Time in seconds before the the econ authentification times out ") },
                { "EcOutputLevel", new ConfigInt("ec_output_level", 1, 0, 2, ConfigFlags.Econ, "Adjusts the amount of information in the external console") },

                { "ClAllowOldServers", new ConfigInt("cl_allow_old_servers", 1, 0, 1, ConfigFlags.Client|ConfigFlags.Server, "Allow connecting to servers that do not furtherly secure the connection") },
                { "Debug", new ConfigInt("debug", 1, 0, 1, ConfigFlags.Client|ConfigFlags.Server, "Debug mode") },
            });
        }

        protected override void AppendVariables(IDictionary<string, ConfigVariable> variables)
        {
            foreach (var pair in variables)
            {
                if (!Variables.TryAdd(pair.Key, pair.Value))
                    Debug.Warning("config", $"Variable '{pair.Key}' already added");
            }
        }

        public override void Save(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public override void RestoreString()
        {
            foreach (var pair in Variables)
            {
                if (pair.Value is ConfigString strCfg)
                {
                    if (string.IsNullOrEmpty(strCfg.Value) &&
                        string.IsNullOrEmpty(strCfg.DefaultValue) == false)
                    {
                        strCfg.Value = strCfg.DefaultValue;
                    }
                }
            }
        }

        protected override void Reset()
        {
            foreach (var pair in Variables)
            {
                if (pair.Value is ConfigString strCfg)
                    strCfg.Value = strCfg.DefaultValue;
                else if (pair.Value is ConfigInt intCfg)
                    intCfg.Value = intCfg.DefaultValue;
            }
        }

        public override IEnumerator<KeyValuePair<string, ConfigVariable>> GetEnumerator()
        {
            return Variables.GetEnumerator();
        }
    }
}