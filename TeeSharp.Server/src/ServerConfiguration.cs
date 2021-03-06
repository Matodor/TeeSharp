// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable ArgumentsStyleLiteral
using TeeSharp.Common.Config;

namespace TeeSharp.Server
{
    public class ServerConfiguration : Configuration
    {
        public ConfigVariableString ServerName { get; } = new ConfigVariableString(
            defaultValue: "[TeeSharp] Unnamed server",
            description: "Server name",
            maxLength: 128
        );
    }
}