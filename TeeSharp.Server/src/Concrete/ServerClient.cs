using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Server.Abstract;
using Uuids;

namespace TeeSharp.Server.Concrete;

public class ServerClient : IServerClient
{
    public int Id { get; }
    public ServerClientState State { get; set; }
    public Uuid? ConnectionUuid { get; set; }

    public string? DDNetVersionString { get; set; }
    public int? DDNetVersion { get; set; }

    protected ILogger Logger { get; set; }

    public ServerClient(
        int id,
        ILogger? logger = null)
    {
        Id = id;
        Logger = logger ?? Tee.LoggerFactory.CreateLogger("NetworkConnection");
    }
}
