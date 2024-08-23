using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TeeSharp.Server;

public class ClientsContainer
{
    public IReadOnlyList<Client> All { get; protected set; }

    protected ILogger Logger { get; set; }
    protected ILoggerFactory LoggerFactory { get; set; }

    public ClientsContainer(int count, ILoggerFactory? loggerFactory = default)
    {
        LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        Logger = LoggerFactory.CreateLogger("Clients");
        All = CreateClientsCollection(count);
    }

    public virtual Client GetByConnectionId(int connectionId)
    {
        return All[connectionId];
    }

    protected virtual IReadOnlyList<Client> CreateClientsCollection(int count)
    {
        return Enumerable.Range(0, count)
            .Select(CreateClient)
            .ToArray();
    }

    protected virtual Client CreateClient(int id)
    {
        return new Client(id, Logger);
    }
}
