// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ArgumentsStyleNamedExpression
using TeeSharp.Common.Config;

namespace TeeSharp.Server
{
    public class ServerConfiguration : Configuration
    {
        public virtual ConfigVariableString ServerName { get; } = new ConfigVariableString(
            defaultValue: "[TeeSharp] Unnamed server",
            description: "Server name",
            maxLength: 128
        );
        
        public virtual ConfigVariableInteger ServerPort { get; } = new ConfigVariableInteger(
            defaultValue: 8303,
            description: "Server port",
            min: ushort.MinValue,
            max: ushort.MaxValue
        );

        public virtual ConfigVariableBoolean UseSixUp { get; } = new ConfigVariableBoolean(
            defaultValue: true,
            description: "Support 0.7+ network protocol"
        );
    }
}