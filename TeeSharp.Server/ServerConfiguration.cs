using TeeSharp.Common.Config;

namespace TeeSharp.Server
{
    public class ServerConfiguration : Configuration
    {
        public ConfigVariableString ServerName { get; } = new ConfigVariableString()
        {
            Description = "Server name",
            DefaultValue = "[TeeSharp] Unnamed server",
            MaxLength = 128,
        };
    }
}