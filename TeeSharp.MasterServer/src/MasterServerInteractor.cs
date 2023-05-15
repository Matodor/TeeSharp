using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using Uuids;

namespace TeeSharp.MasterServer;

public class MasterServerInteractor
{
    protected const int UuidStrSizeD = 36;

    public Uuid Secret { get; }
    public Uuid ChallengeSecret { get; }
    public int Port { get; private set; } = 8303;
    public MasterServerRegisterResponseStatus LatestResponseStatus { get; private set; }

    protected readonly IReadOnlyDictionary<MasterServerProtocolType, MasterServerInteractorProtocol> Protocols;
    protected readonly ILogger Logger;
    protected readonly byte[] VerifyChallengeSecretData;

    private ServerInfo? _serverInfo = null;
    private int _serverInfoSerial = 0;
    private object _serverInfoLock = new();

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
        }.ToDictionary(t => t, t => new MasterServerInteractorProtocol(this, t));
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

    public void UpdateServerInfo(ServerInfo info)
    {
        if (_serverInfo != null &&
            _serverInfo.Equals(info) != false)
        {
            Logger.LogInformation("UpdateServerInfo: ignore");
            return;
        }

        _serverInfo = info;
        _serverInfoSerial++;

        var json = JsonSerializer.Serialize(_serverInfo);

        foreach (var protocol in Protocols.Values)
        {
            protocol.SendInfo(json, _serverInfoSerial).ContinueWith(ContinuationRegister);
        }
    }

    private void ContinuationRegister(Task<MasterServerRegisterResponseStatus?> obj)
    {
        // throw new NotImplementedException();
    }

    public bool ProcessMasterServerPacket(Span<byte> data, IPEndPoint endPoint)
    {
        Logger.LogInformation("ProcessMasterServerPacket: {Msg}", Encoding.ASCII.GetString(data));

        if (data.Length < VerifyChallengeSecretData.Length ||
            data.Slice(0, VerifyChallengeSecretData.Length).SequenceEqual(VerifyChallengeSecretData) == false)
        {
            return false;
        }

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
}
