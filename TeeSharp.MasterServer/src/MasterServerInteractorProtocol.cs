using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;

namespace TeeSharp.MasterServer;

public class MasterServerInteractorProtocol
{
    public bool Enabled { get; set; }
    public MasterServerProtocolType Type { get; }
    public DateTime LastRequest { get; private set; }
    public DateTime NextRequest { get; private set; }

    protected string? ChallengeToken { get; set; }
    protected readonly ILogger Logger;

    private readonly HttpClient _client;
    private readonly MasterServerInteractor _interactor;

    public MasterServerInteractorProtocol(
        MasterServerInteractor interactor,
        MasterServerProtocolType type,
        HttpClient client,
        ILogger? logger = null)
    {
        _client = client;
        _interactor = interactor;

        Logger = logger ?? Tee.LoggerFactory.CreateLogger(nameof(MasterServerInteractor));
        Type = type;
    }

    protected HttpRequestMessage GetRequest()
    {
        return new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Headers =
            {
                { MasterServerInteractorHeaders.Address, GetHeaderAddress() },
                { MasterServerInteractorHeaders.ChallengeSecret, GetHeaderChallengeSecret() },
            },
        };
    }

    public async Task<MasterServerResponse> SendInfoAsync(string infoJson)
    {
        var request = GetRequest();

        request.Content = new StringContent(infoJson, Encoding.UTF8,
            new MediaTypeHeaderValue(MediaTypeNames.Application.Json, null));

        if (ChallengeToken != null)
        {
            request.Headers.Add(
                name: MasterServerInteractorHeaders.ChallengeToken,
                value: ChallengeToken
            );
        }

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        await using var contentStream = await response.Content.ReadAsStreamAsync();

        LastRequest = DateTime.UtcNow;
        NextRequest = LastRequest.AddSeconds(15);

        using var jsonDocument = await JsonDocument.ParseAsync(contentStream);

        var jsonResponse = jsonDocument.RootElement;
        if (jsonResponse.TryGetProperty("status", out var elementStatus) == false ||
            elementStatus.ValueKind != JsonValueKind.String)
        {
            Logger.LogWarning("Invalid json response from masterserver");
            return new MasterServerResponse(MasterServerResponseCode.Error);
        }

        var statusStr = elementStatus.GetString()!;
        if (statusStr == "error")
        {
            if (jsonResponse.TryGetProperty("message", out var messageProp) &&
                messageProp.ValueKind == JsonValueKind.String)
            {
                Logger.LogWarning("Got error from masterserver ({Type}:{Status})", Type, messageProp.GetString()!);
            }

            return new MasterServerResponse(MasterServerResponseCode.Error);
        }

        if (MasterServerHelper.TryParseRegisterResponseStatus(statusStr, out var status) == false)
        {
            Logger.LogWarning("Invalid status from masterserver ({Type}:{Status})", Type, statusStr);
            return new MasterServerResponse(MasterServerResponseCode.Error);
        }

        Logger.LogInformation("Got response from masterserver ({Type}:{Status})", Type, status);
        return new MasterServerResponse(status);
    }

    protected string GetHeaderChallengeSecret()
    {
        return $"{_interactor.ChallengeSecret.ToString("d")}:{Type.ToStringRepresentation()}";
    }

    protected string GetHeaderAddress()
    {
        return $"{Type.ToScheme()}connecting-address.invalid:{_interactor.Port}";
    }

    public void ProcessToken(string token)
    {
        ChallengeToken = token;
        Logger.LogInformation("ProcessToken: got token {Protocol}:{Token}", Type, ChallengeToken);
    }
}
