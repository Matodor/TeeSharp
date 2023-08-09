using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;

namespace TeeSharp.MasterServer;

public class MasterServerInteractorProtocol
{
    public bool Enabled { get; set; }
    public MasterServerProtocolType Type { get; }

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

    protected string GetHeaderAddress()
    {
        return $"{Type.ToScheme()}connecting-address.invalid:{_interactor.Port}";
    }

    protected string GetHeaderChallengeSecret()
    {
        return $"{_interactor.ChallengeSecret.ToString("d")}:{Type.ToStringRepresentation()}";
    }

    public async Task<MasterServerResponse> RegisterAsync(
        string? serverInfo,
        CancellationToken cancellationToken)
    {
        var request = GetRequest();

        if (serverInfo != null)
        {
            request.Content = new StringContent(
                content: serverInfo,
                encoding: Encoding.UTF8,
                mediaType: new MediaTypeHeaderValue(MediaTypeNames.Application.Json, null)
            );
        }

        using var response = await _client
            .SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
            .ConfigureAwait(false);

        await using var contentStream = await response.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        using var jsonDocument = await JsonDocument
            .ParseAsync(contentStream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var jsonResponse = jsonDocument.RootElement;
        if (jsonResponse.TryGetProperty("status", out var elementStatus) == false ||
            elementStatus.ValueKind != JsonValueKind.String)
        {
            Logger.LogWarning("Invalid json response from masterserver");
            return new MasterServerResponse(false);
        }

        var statusStr = elementStatus.GetString()!;
        if (statusStr == "error")
        {
            if (jsonResponse.TryGetProperty("message", out var messageProp) &&
                messageProp.ValueKind == JsonValueKind.String)
            {
                Logger.LogWarning("Got error from masterserver ({Type}:{Status})", Type, messageProp.GetString()!);
            }

            return new MasterServerResponse(false);
        }

        if (MasterServerHelper.TryParseRegisterResponseStatus(statusStr, out var status) == false)
        {
            Logger.LogWarning("Invalid status from masterserver ({Type}:{Status})", Type, statusStr);
            return new MasterServerResponse(false);
        }

        return new MasterServerResponse(true, status);
    }
}
