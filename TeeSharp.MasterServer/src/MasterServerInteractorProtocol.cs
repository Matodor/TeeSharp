using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;

namespace TeeSharp.MasterServer;

public class MasterServerInteractorProtocol
{
    public MasterServerProtocolType Type { get; }
    public bool Enabled { get; set; }
    public DateTime LastRequest { get; private set; }
    public DateTime NextRequest { get; private set; }

    protected string? ChallengeToken { get; set; }
    protected readonly ILogger Logger;

    private readonly MasterServerInteractor _interactor;

    public MasterServerInteractorProtocol(
        MasterServerInteractor interactor,
        MasterServerProtocolType type,
        ILogger? logger = null)
    {
        _interactor = interactor;

        Logger = logger ?? Tee.LoggerFactory.CreateLogger(nameof(MasterServerInteractor));
        Type = type;
    }

    public async Task<MasterServerResponse?> SendInfo(string infoJson, int infoSerial)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:9090/ddnet/15/register"),
            // BaseAddress = new Uri("https://master1.ddnet.org/ddnet/15/register"),
        };

        var requestContent = new StringContent(infoJson, Encoding.UTF8);
        requestContent.Headers.ContentType!.MediaType = "application/json";
        requestContent.Headers.ContentType!.CharSet = null;

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = requestContent,
            Headers =
            {
                {"Address", GetHeaderAddress()},
                {"Secret", GetHeaderSecret()},
                {"Challenge-Secret", GetHeaderChallengeSecret()},
                {"Info-Serial", infoSerial.ToString()},
            },
        };

        if (ChallengeToken != null)
            request.Headers.Add("Challenge-Token", ChallengeToken);

        Logger.LogInformation("Test: headers - {Headers}", request.Headers.ToString());

        using var result = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        await using var contentStream = await result.Content.ReadAsStreamAsync();

        LastRequest = DateTime.UtcNow;
        NextRequest = LastRequest.AddSeconds(15);

        JsonDocument jsonDocument;
        JsonElement jsonResponse;

        try
        {
            jsonDocument = await JsonDocument.ParseAsync(contentStream);
            jsonResponse = jsonDocument.RootElement;
        }
        catch (JsonException e)
        {
            // do nothing
            throw;
        }

        if (jsonResponse.TryGetProperty("status", out var elementStatus) == false ||
            elementStatus.ValueKind != JsonValueKind.String)
        {
            Logger.LogWarning("Invalid json response from masterserver");
            return null;
        }

        var statusStr = elementStatus.GetString()!;
        if (statusStr == "error")
        {
            if (jsonResponse.TryGetProperty("message", out var elementMessage) &&
                elementMessage.ValueKind == JsonValueKind.String)
            {
                Logger.LogWarning("Got error from masterserver ({Status})", elementMessage.GetString()!);
            }

            return null;
        }

        if (MasterServerHelper.TryParseRegisterResponseStatus(statusStr, out var status) == false)
        {
            Logger.LogWarning("Invalid status from masterserver ({Status})", statusStr);
            return null;
        }

        jsonDocument.Dispose();
        client.Dispose();

        return new MasterServerResponse(infoSerial, status);
    }

    protected string GetHeaderSecret()
    {
        return _interactor.Secret.ToString("d");
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
