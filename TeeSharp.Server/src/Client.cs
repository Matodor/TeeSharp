using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Uuids;

namespace TeeSharp.Server;

public delegate void ClientAction(Client client);
public delegate void ClientStateAction(Client client, ClientState now, ClientState prev);

public class Client
{
    public event ClientStateAction StateChanged = delegate { };

    public bool GotDDNetVersion => VersionDDNetNum.HasValue;

    public int Id { get; }
    public ClientState State { get; private set; }
    public Uuid? ConnectionUuid { get; private set; }
    public string Version { get; private set; } = string.Empty;
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
        Version = string.Empty;
        VersionDDNet = null;
        VersionDDNetNum = null;

        return this;
    }

    public virtual Client SetState(ClientState state)
    {
        if (State == state)
            return this;

        var prev = State;

        State = state;
        StateChanged(this, State, prev);

        return this;
    }

    public virtual Client SetConnectionUuid(Uuid? connectionUuid)
    {
        ConnectionUuid = connectionUuid;
        return this;
    }

    public virtual Client SetVersion(string version)
    {
        Version = version;
        return this;
    }

    public virtual Client SetDDNetVersion(string? version, int? versionNum)
    {
        VersionDDNet = version;
        VersionDDNetNum = versionNum;
        return this;
    }
}
