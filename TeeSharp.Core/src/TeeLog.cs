using Microsoft.Extensions.Logging;

namespace TeeSharp.Core;

public static partial class Tee
{
    public static ILogger Logger { get; set; } = null!;
    public static ILoggerFactory LoggerFactory { get; set; } = null!;
}
