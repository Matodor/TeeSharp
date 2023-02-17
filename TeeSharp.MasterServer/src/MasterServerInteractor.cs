using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using Uuids;

namespace TeeSharp.MasterServer;

public class MasterServerInteractor
{
    public Uuid Secret { get; }
    public Uuid ChallengeSecret { get; }

    protected readonly IReadOnlyDictionary<MasterServerProtocolType, MasterServerProtocol> Protocols;
    protected readonly ILogger Logger;

    public MasterServerInteractor(ILogger? logger = null)
    {
        Secret = Uuid.NewTimeBased();
        ChallengeSecret = Uuid.NewTimeBased();

        Logger = logger ?? Tee.LoggerFactory.CreateLogger(nameof(MasterServerInteractor));
        Protocols = new[]
        {
            MasterServerProtocolType.SixIPv4,
            // MasterServerProtocolType.SixIPv6,
            // MasterServerProtocolType.SixupIPv4,
            // MasterServerProtocolType.SixupIPv6,
        }.ToDictionary(t => t, t => new MasterServerProtocol(this, t));
    }

    public async Task UpdateServerInfo(ServerInfo info)
    {
        foreach (var protocol in Protocols.Values)
        {
            await protocol.Test();
        }
    }
}
