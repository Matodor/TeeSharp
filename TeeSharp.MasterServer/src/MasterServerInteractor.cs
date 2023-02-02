using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;

namespace TeeSharp.MasterServer;

public class MasterServerInteractor
{
    protected readonly IReadOnlyDictionary<MasterServerProtocolType, MasterServerProtocol> Protocols;
    protected readonly ILogger? Logger;

    public MasterServerInteractor(ILogger? logger = null)
    {
        Logger = logger ?? Tee.LoggerFactory.CreateLogger(nameof(MasterServerInteractor));
        Protocols = new[]
        {
            MasterServerProtocolType.SixIPv4,
            MasterServerProtocolType.SixIPv6,
            MasterServerProtocolType.SixupIPv4,
            MasterServerProtocolType.SixupIPv6,
        }.ToDictionary(t => t, t => new MasterServerProtocol(t));
    }

    public void ApplySettings()
    {

    }
}
