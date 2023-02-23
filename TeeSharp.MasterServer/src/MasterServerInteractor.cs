using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Core.Extensions;
using TeeSharp.Core.Helpers;
using TeeSharp.Network;
using Uuids;

namespace TeeSharp.MasterServer;

public class MasterServerInteractor
{
    protected const int UuidStrSizeD = 36;

    public Uuid Secret { get; }
    public Uuid ChallengeSecret { get; }

    protected readonly IReadOnlyDictionary<MasterServerProtocolType, MasterServerProtocol> Protocols;
    protected readonly ILogger Logger;

    protected readonly byte[] VerifyChallengeSecretData;

    public MasterServerInteractor(ILogger? logger = null)
    {
        Secret = Uuid.NewTimeBased();
        ChallengeSecret = Uuid.NewTimeBased();
        VerifyChallengeSecretData = GetVerifyChallengeSecretData();

        Logger = logger ?? Tee.LoggerFactory.CreateLogger(nameof(MasterServerInteractor));
        Protocols = new[]
        {
            MasterServerProtocolType.SixIPv4,
            // MasterServerProtocolType.SixIPv6,
            // MasterServerProtocolType.SixupIPv4,
            // MasterServerProtocolType.SixupIPv6,
        }.ToDictionary(t => t, t => new MasterServerProtocol(this, t));
    }

    protected byte[] GetVerifyChallengeSecretData()
    {
        var data = (Span<byte>)new byte[MasterServerPackets.Challenge.Length + UuidStrSizeD + 1];
        var challengePacket = MasterServerPackets.Challenge.AsSpan();
        var challengeSecret = Encoding.ASCII.GetBytes(ChallengeSecret.ToString("d") + ":").AsSpan();

        challengePacket.CopyTo(data);
        challengeSecret.CopyTo(data.Slice(MasterServerPackets.Challenge.Length));

        return data.ToArray();
    }

    public ServerInfo ServerInfo { get; set; }

    public async Task UpdateServerInfo(ServerInfo info)
    {
        ServerInfo = info;

        foreach (var protocol in Protocols.Values)
        {
            await protocol.Test(false);
        }
    }

    public bool ProcessMasterServerPacket(Span<byte> data, IPEndPoint endPoint)
    {
        Logger.LogInformation("ProcessMasterServerPacket: {Msg}", Encoding.ASCII.GetString(data));

        if (data.Length >= VerifyChallengeSecretData.Length &&
            data.Slice(0, VerifyChallengeSecretData.Length).SequenceEqual(VerifyChallengeSecretData))
        {
            var unpacker = new Unpacker(data.Slice(VerifyChallengeSecretData.Length));
            if (unpacker.TryGetString(out var protocolStr) == false ||
                unpacker.TryGetString(out var token) == false)
            {
                Logger.LogInformation("ProcessMasterServerPacket: Can't unpack protocol and token");
                return false;
            }

            if (!MasterServerHelper.TryParseProtocolType(protocolStr, out var protocolType))
            {
                Logger.LogInformation("ProcessMasterServerPacket: Unknown protocol type");
                return false;
            }

            if (!Protocols.TryGetValue(protocolType, out var protocol))
            {
                Logger.LogInformation("ProcessMasterServerPacket: Unsupported protocol type");
                return false;
            }

            protocol.ProcessToken(token);
            return true;
        }

        return false;
    }
}
