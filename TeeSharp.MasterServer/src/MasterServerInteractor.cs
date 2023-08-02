using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using Uuids;

namespace TeeSharp.MasterServer;

public class MasterServerInteractor : IDisposable
{
    public Uuid Secret { get; }
    public Uuid ChallengeSecret { get; }

    public int Port { get; private set; } = 8303;

    public required Uri Endpoint
    {
        get => _httpClient.BaseAddress ?? throw new NullReferenceException(nameof(_httpClient.BaseAddress));
        set => _httpClient.BaseAddress = value;
    }

    protected readonly IReadOnlyDictionary<MasterServerProtocolType, MasterServerInteractorProtocol> Protocols;
    protected readonly ILogger Logger;
    protected readonly byte[] VerifyChallengeSecretData;

    private MasterServerResponseCode _latestResponseCode = MasterServerResponseCode.None;
    private int _latestInfoSerial = -1;

    private ServerInfo? _serverInfo;
    private int _serverInfoSerial;
    private int _totalRequests = 0;
    private object _serverInfoLock = new();
    private readonly HttpClient _httpClient;

    public MasterServerInteractor(ILogger? logger = null)
    {
        Secret = Uuid.NewTimeBased();
        ChallengeSecret = Uuid.NewTimeBased();
        VerifyChallengeSecretData = GetVerifyChallengeSecretData();
        Logger = logger ?? Tee.LoggerFactory.CreateLogger(nameof(MasterServerInteractor));

        _httpClient = CreateClient();

        Protocols = new[]
        {
            MasterServerProtocolType.SixIPv4,
            MasterServerProtocolType.SixIPv6,
            MasterServerProtocolType.SixupIPv4,
            MasterServerProtocolType.SixupIPv6,
        }.ToDictionary(
            t => t,
            t => new MasterServerInteractorProtocol(this, t, _httpClient)
            {
                Enabled = true,
            }
        );
    }

    protected virtual HttpClient CreateClient()
    {
        return new HttpClient
        {
            DefaultRequestHeaders =
            {
                { MasterServerInteractorHeaders.Secret, GetHeaderSecret() },
                { MasterServerInteractorHeaders.InfoSerial, _serverInfoSerial.ToString() },
            },
        };
    }

    protected byte[] GetVerifyChallengeSecretData()
    {
        var challengePacket = MasterServerPackets.Challenge.AsSpan();
        var challengeSecret = Encoding.ASCII.GetBytes(ChallengeSecret.ToString("d") + ":").AsSpan();
        var data = (Span<byte>)new byte[challengePacket.Length + challengeSecret.Length];

        challengePacket.CopyTo(data);
        challengeSecret.CopyTo(data.Slice(challengePacket.Length));

        return data.ToArray();
    }

    public void UpdateServerInfo(ServerInfo info)
    {
        if (_serverInfo != null &&
            _serverInfo.Equals(info))
        {
            Logger.LogInformation("UpdateServerInfo: ignore");
            return;
        }

        _serverInfo = info;
        _serverInfoSerial++;

        var json = JsonSerializer.Serialize(_serverInfo);

        foreach (var protocol in Protocols.Values)
        {
            protocol
                .SendInfoAsync(json)
                .ContinueWith(ProcessResponse, (++_totalRequests, _serverInfoSerial))
                .ConfigureAwait(false);
        }
    }

    protected string GetHeaderSecret()
    {
        return Secret.ToString("d");
    }

    private void ProcessResponse(Task<MasterServerResponse> task, object? state)
    {
        var (requestId, infoSerial) = ((int, int))state!;

        // if (task.Exception != null)
        //     throw task.Exception;

        // throw new NotImplementedException();
    }

    public bool ProcessMasterServerPacket(Span<byte> data, IPEndPoint endPoint)
    {
        Logger.LogInformation("ProcessMasterServerPacket: {Msg}", Encoding.ASCII.GetString(data));

        if (data.Length < VerifyChallengeSecretData.Length ||
            data.Slice(0, VerifyChallengeSecretData.Length).SequenceEqual(VerifyChallengeSecretData) == false)
        {
            Logger.LogInformation("ProcessMasterServerPacket: Got erroneous challenge packet");
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

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
