// ReSharper disable ArgumentsStyleStringLiteral
using TeeSharp.Common.Config;

namespace TeeSharp.Server
{
    public class ServerConfiguration : Configuration
    {
        public ConfigVariableString ServerName { get; } = new ConfigVariableString(
            defaultValue: "[TeeSharp] Unnamed server",
            description: "Server name",
            maxLength: 4
        );
    }
}