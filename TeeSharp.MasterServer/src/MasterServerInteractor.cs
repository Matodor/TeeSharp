using System;
using System.Collections.Generic;
using System.Linq;
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
        var challengePacketSpan = MasterServerPackets.Challenge.AsSpan();
        var challengeSecretSpan = Encoding.ASCII.GetBytes(ChallengeSecret.ToGuidString() + ":");

        challengePacketSpan.CopyTo(data);
        challengeSecretSpan.CopyTo(data.Slice(MasterServerPackets.Challenge.Length));

        return data.ToArray();
    }

    public async Task UpdateServerInfo(ServerInfo info)
    {
        foreach (var protocol in Protocols.Values)
        {
            await protocol.Test();
        }
    }

    public bool ProcessNetworkMessage(NetworkMessage message)
    {
        // ????chalbe48b69f-b08a-11ed-9bf1-57c3acfe7d93:tw0.6/ipv4p+BmhrnUMfiGTUTcsoLHow==

        Logger.LogInformation("ProcessNetworkMessage: {Msg}", Encoding.ASCII.GetString(message.Data));
        return false;

        // if (message.)
        // if (message.ExtraData.Length > 0 &&
        //     MasterServerPackets.GetInfo.Length + 1 <= message.Data.Length &&
        //     MasterServerPackets.GetInfo.AsSpan()
        //         .SequenceEqual(message.Data.AsSpan(0, MasterServerPackets.GetInfo.Length)))
        // {
        //     var extraToken = ((message.ExtraData[0] << 8) | message.ExtraData[1]) << 8;
        //     var token = (SecurityToken) (message.Data[MasterServerPackets.GetInfo.Length] | extraToken);
        //
        //     SendServerInfoConnectionLess(message.EndPoint, token);
        // }
    }
}
