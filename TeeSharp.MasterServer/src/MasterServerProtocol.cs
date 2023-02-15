using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace TeeSharp.MasterServer;

public class MasterServerProtocol
{
    public MasterServerProtocolType Type { get; }
    public bool Enabled { get; set; }

    public MasterServerProtocol(MasterServerProtocolType type)
    {
        Type = type;
    }

    public void Test()
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
                {"Address", "test"},
                {"Secret", "test"},
                {"Challenge-Secret", "test"},
                {"Info-Serial", "0"},
            },
        };


        var result = client.SendAsync(request).GetAwaiter().GetResult();
        var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}
