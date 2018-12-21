using System.Collections;
using System.Collections.Generic;
using TeeSharp.Core;

namespace TeeSharp.Common.Config
{
    public abstract class BaseConfig : BaseInterface, IEnumerable<KeyValuePair<string, ConfigVariable>>
    {
        public virtual ConfigVariable this[string key] => Variables[key];

        protected virtual IDictionary<string, ConfigVariable> Variables { get; set; }

        protected BaseConfig()
        {
            Variables = new Dictionary<string, ConfigVariable>();

            AppendVariables(new Dictionary<string, ConfigVariable>()
            {
                { "ConnTimeout", new ConfigInt("ConnTimeout", "conn_timeout", 100, 5, 1000, ConfigFlags.SAVE | ConfigFlags.CLIENT | ConfigFlags.SERVER, "Network timeout") },
                { "ConnTimeoutProtection", new ConfigInt("ConnTimeoutProtection", "conn_timeout_protection", 1000, 5, 10000, ConfigFlags.SAVE | ConfigFlags.CLIENT | ConfigFlags.SERVER, "Network timeout protection") },
                { "Password", new ConfigString("Password", "password", 32, "", ConfigFlags.CLIENT|ConfigFlags.SERVER, "Password to the server") },
                { "Logfile", new ConfigString("Logfile", "logfile", 128, "", ConfigFlags.SAVE|ConfigFlags.CLIENT|ConfigFlags.SERVER, "Filename to log all output to") },
                { "ConsoleOutputLevel", new ConfigInt("ConsoleOutputLevel", "console_output_level", 0, 0, 2, ConfigFlags.CLIENT|ConfigFlags.SERVER, "Adjusts the amount of information in the console") },
                { "Bindaddr", new ConfigString("Bindaddr", "bindaddr", 128, "", ConfigFlags.CLIENT|ConfigFlags.SERVER|ConfigFlags.MASTER, "Address to bind the client/server to") },

                { "EcBindaddr", new ConfigString("EcBindaddr", "ec_bindaddr", 128, "localhost", ConfigFlags.ECON, "Address to bind the external console to. Anything but 'localhost' is dangerous") },
                { "EcPort", new ConfigInt("EcPort", "ec_port", 0, 0, 0, ConfigFlags.ECON, "Port to use for the external console") },
                { "EcPassword", new ConfigString("EcPassword", "ec_password", 32, "", ConfigFlags.ECON, "External console password") },
                { "EcBantime", new ConfigInt("EcBantime", "ec_bantime", 0, 0, 1440, ConfigFlags.ECON, "The time a client gets banned if econ authentication fails. 0 just closes the connection") },
                { "EcAuthTimeout", new ConfigInt("EcAuthTimeout", "ec_auth_timeout", 30, 1, 120, ConfigFlags.ECON, "Time in seconds before the the econ authentification times out ") },
                { "EcOutputLevel", new ConfigInt("EcOutputLevel", "ec_output_level", 1, 0, 2, ConfigFlags.ECON, "Adjusts the amount of information in the external console") },

                { "ClAllowOldServers", new ConfigInt("ClAllowOldServers", "cl_allow_old_servers", 1, 0, 1, ConfigFlags.CLIENT|ConfigFlags.SERVER, "Allow connecting to servers that do not furtherly secure the connection") },
                { "Debug", new ConfigInt("Debug", "debug", 1, 0, 1, ConfigFlags.CLIENT|ConfigFlags.SERVER, "Debug mode") },
            });
        }

        protected virtual void AppendVariables(IDictionary<string, ConfigVariable> variables)
        {
            foreach (var pair in variables)
            {
                if (!Variables.TryAdd(pair.Key, pair.Value))
                    Debug.Warning("config", $"Variable '{pair.Key}' already added");
            }
        }

        protected virtual void Reset()
        {
            foreach (var pair in Variables)
            {
                if (pair.Value is ConfigString strCfg)
                    strCfg.Value = strCfg.DefaultValue;
                else if (pair.Value is ConfigInt intCfg)
                    intCfg.Value = intCfg.DefaultValue;
            }
        }

        public IEnumerator<KeyValuePair<string, ConfigVariable>> GetEnumerator()
        {
            return Variables.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}