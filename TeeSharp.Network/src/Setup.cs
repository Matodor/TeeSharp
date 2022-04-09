using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeeSharp.Network;

public static class Setup
{
    public static IServiceCollection AddNetwork(
        this IServiceCollection services, IConfiguration config)
    {
        return services.AddSingleton<INetworkServer, NetworkServer>();
    }
}
