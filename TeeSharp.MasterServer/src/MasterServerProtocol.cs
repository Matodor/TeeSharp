using System;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Core.Extensions;

namespace TeeSharp.MasterServer;

public class MasterServerProtocol
{
    public MasterServerProtocolType Type { get; }
    public bool Enabled { get; set; }

    protected readonly ILogger Logger;

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

    public async Task Test()
    {
        var client = new HttpClient()
        {
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

        // if(m_HaveChallengeToken)
        // {
        //     pRegister->HeaderString("Challenge-Token", m_aChallengeToken);
        // }

        Logger.LogDebug("Test: {Headers}", request.Headers.ToString());

        var result = await client.SendAsync(request);
        var content = await result.Content.ReadAsStringAsync();
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
}
