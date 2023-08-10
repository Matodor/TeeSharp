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
using TeeSharp.Core.Extensions;
using Uuids;

namespace TeeSharp.MasterServer;

public class MasterServerInteractor : IDisposable
{
    public Uuid Secret { get; }
    public Uuid ChallengeSecret { get; }

    public string? ChallengeToken
    {
        get => _challengeToken;
        private set => SetChallengeToken(value);
    }

    public int Port { get; private set; } = 8303;

    public required Uri Endpoint
    {
        get => _httpClient.BaseAddress ?? throw new NullReferenceException(nameof(_httpClient.BaseAddress));
        set => _httpClient.BaseAddress = value;
    }

    protected readonly CancellationToken CancellationToken;
    protected readonly IReadOnlyDictionary<MasterServerProtocolType, MasterServerInteractorProtocol> Protocols;
    protected readonly ILogger Logger;
    protected readonly byte[] VerifyChallengeSecretData;

    private MasterServerResponseCode _latestResponseCode = MasterServerResponseCode.None;
    private int _latestInfoSerial = -1;
    private int _latestRequestId = -1;

    private DateTime _prevRequest;
    private DateTime _nextRequest;
    private Task? _registerTask;
    private ServerInfo? _serverInfo;
    private int _serverInfoSerial;
    private int _totalRequests;

    private readonly HttpClient _httpClient;
    private readonly object _responseLock = new();
    private string? _challengeToken;

    public MasterServerInteractor(CancellationToken cancellationToken, ILogger? logger = null)
    {
        CancellationToken = cancellationToken;
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

    protected string GetHeaderSecret()
    {
        return Secret.ToString("d");
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

    public void Update()
    {
        if (_serverInfo == null ||
            _registerTask is { IsCompleted: false })
        {
            return;
        }

        var sendRegister = false;
        var sendInfo = false;

        lock (_responseLock)
        {
            if (_nextRequest < DateTime.Now)
            {
                sendRegister = true;
                sendInfo =
                    _latestResponseCode == MasterServerResponseCode.NeedInfo ||
                    _latestInfoSerial != _serverInfoSerial;
            }
        }

        if (sendRegister)
            SendRegister(sendInfo: sendInfo);
    }

    public void UpdateServerInfo(ServerInfo info)
    {
        if (_serverInfo != null &&
            _serverInfo.Equals(info))
        {
            Logger.LogTrace("UpdateServerInfo: Ignore");
            return;
        }

        Logger.LogDebug("Got new server info");

        _serverInfo = info;
        _serverInfoSerial++;

        lock (_responseLock)
        {
            _nextRequest = new DateTime(Math.Min(
                _prevRequest.Ticks + TimeSpan.FromSeconds(1).Ticks,
                _nextRequest.Ticks
            ));
        }
    }

    public void SendRegister(bool sendInfo)
    {
        if (_serverInfo == null)
            return;

        Logger.LogDebug("Registering...");

        var json = sendInfo
            ? JsonSerializer.Serialize(_serverInfo)
            : null;

        lock (_responseLock)
        {
            _prevRequest = DateTime.Now;
            _nextRequest = DateTime.Now + TimeSpan.FromSeconds(15);
        }

        _registerTask = Task.WhenAll(
            Protocols.Values
                .Where(protocol => protocol.Enabled)
                .Select(protocol => protocol
                    .RegisterAsync(json, CancellationToken)
                    .ContinueWith(ProcessResponse, (++_totalRequests, _serverInfoSerial), CancellationToken)
                    .Tap(t => t.ConfigureAwait(false))
                )
        );
    }

    private void ProcessResponse(Task<MasterServerResponse> task, object? state)
    {
        if (task.Exception != null)
        {
            Logger.LogCritical(task.Exception, "SendInfoAsync an exception was thrown");
            return;
        }

        if (task.Result.Successful == false)
            return;

        var (requestId, infoSerial) = ((int, int))state!;

        lock (_responseLock)
        {
            if (_latestRequestId < requestId)
            {
                _latestRequestId = requestId;
                _latestResponseCode = task.Result.Code;

                if (_latestResponseCode == MasterServerResponseCode.Ok &&
                    _latestInfoSerial < infoSerial)
                {
                    _latestInfoSerial = infoSerial;
                }
                else if (_latestResponseCode == MasterServerResponseCode.NeedInfo)
                {
                    _nextRequest = DateTime.Now;
                }

                Logger.LogDebug("Got response (latest): RequestId={RequestId} Code={Code} Info={InfoSerial}", requestId, task.Result.Code, infoSerial);
            }
        }
    }

    public bool ProcessMasterServerPacket(Span<byte> data, IPEndPoint endPoint)
    {
        if (data.Length < VerifyChallengeSecretData.Length ||
            data.Slice(0, VerifyChallengeSecretData.Length).SequenceEqual(VerifyChallengeSecretData) == false)
        {
            Logger.LogDebug("ProcessMasterServerPacket: Got erroneous challenge packet");
            return false;
        }

        var unpacker = new Unpacker(data.Slice(VerifyChallengeSecretData.Length));
        if (unpacker.TryGetString(out var protocolStr) == false ||
            unpacker.TryGetString(out var token) == false)
        {
            Logger.LogDebug("ProcessMasterServerPacket: Can't unpack protocol and token");
            return true;
        }

        if (!MasterServerHelper.TryParseProtocolType(protocolStr, out var protocolType))
        {
            Logger.LogDebug("ProcessMasterServerPacket: Unknown protocol type");
            return true;
        }

        if (!Protocols.TryGetValue(protocolType, out var protocol))
        {
            Logger.LogDebug("ProcessMasterServerPacket: Unsupported protocol type");
            return true;
        }

        if (ChallengeToken == token)
            return true;

        SetChallengeToken(token);
        Logger.LogDebug("Got challenge token: Protocol={Protocol} Token={Token}", protocol.Type, token);

        lock (_responseLock)
        {
            if (_latestResponseCode == MasterServerResponseCode.NeedChallenge)
            {
                _nextRequest = DateTime.Now;
            }
        }

        return true;
    }

    private void SetChallengeToken(string? token)
    {
        _challengeToken = token;
        _httpClient.DefaultRequestHeaders.Remove(MasterServerInteractorHeaders.ChallengeToken);

        if (token != null)
            _httpClient.DefaultRequestHeaders.Add(MasterServerInteractorHeaders.ChallengeToken, token);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient.Dispose();
        }
    }
}
