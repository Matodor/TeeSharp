using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Uuids;

namespace TeeSharp.Server;

public delegate void ClientDelegate(Client client);

public class Client
{
    public event ClientDelegate StateChanged = delegate { };

    public int Id { get; }

    public ClientState State { get; private set; }
    public Uuid? ConnectionUuid { get; private set; }
    public string? VersionDDNet { get; private set; }
    public int? VersionDDNetNum { get; private set; }

    protected ILogger Logger { get; set; }

    public Client(int id, ILogger? logger = null)
    {
        Id = id;
        State = ClientState.Empty;
        Logger = logger ?? NullLogger.Instance;
    }

    public virtual Client Reset()
    {
        State = ClientState.Empty;
        ConnectionUuid = null;
        VersionDDNet = null;
        VersionDDNetNum = null;

        return this;
    }

    public virtual Client SetState(ClientState state)
    {
        if (State == state)
            return this;

        State = state;
        StateChanged(this);
    }
}
