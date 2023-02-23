using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;

namespace TeeSharp.MasterServer;

public class MasterServerProtocol
{
    public MasterServerProtocolType Type { get; }
    public bool Enabled { get; set; }

    protected readonly ILogger Logger;
    protected string? ChallengeToken { get; set; }

    private readonly MasterServerInteractor _interactor;

    public MasterServerProtocol(
        MasterServerInteractor interactor,
        MasterServerProtocolType type,
        ILogger? logger = null)
    {
        _interactor = interactor;

        Logger = logger ?? Tee.LoggerFactory.CreateLogger(nameof(MasterServerInteractor));
        Type = type;
    }

    public async Task Test(bool sendInfo)
    {
        var client = new HttpClient()
        {
            // BaseAddress = new Uri("http://127.0.0.1:8080/ddnet/15/register"),
            BaseAddress = new Uri("https://master1.ddnet.org/ddnet/15/register"),
        };

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Headers =
            {
                {"Address", GetHeaderAddress()},
                {"Secret", GetHeaderSecret()},
                {"Challenge-Secret", GetHeaderChallengeSecret()},
                {"Info-Serial", "0"},
            },
        };

        if (sendInfo)
        {
            var json = JsonSerializer.Serialize(_interactor.ServerInfo);
            request.Content = new StringContent(json, Encoding.UTF8);
            request.Content.Headers.ContentType!.MediaType = "application/json";
            request.Content.Headers.ContentType!.CharSet = null;
        }

        if (ChallengeToken != null)
            request.Headers.Add("Challenge-Token", ChallengeToken);

        Logger.LogInformation("Test: headers - {Headers}", request.Headers.ToString());

        var result = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        var content = await result.Content.ReadAsStringAsync();

        Logger.LogInformation("Test: content - {Content}", content);
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
        return $"{Type.ToScheme()}connecting-address.invalid:{8303}";
    }

    public void ProcessToken(string token)
    {
        ChallengeToken = token;
        Logger.LogInformation("ProcessToken: got token {Protocol}:{Token}", Type, ChallengeToken);

        Test(true).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
