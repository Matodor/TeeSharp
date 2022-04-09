using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeeSharp.Commands;
using TeeSharp.Network;

namespace TeeSharp.Server;

public static class Setup
{
    public static IServiceCollection AddGameServer(
        this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddServerSettings(config)
            .AddNetwork(config)
            .AddCommands();
    }

    private static IServiceCollection AddServerSettings(
        this IServiceCollection services, IConfiguration config)
    {
        var settingsSection = config.GetSection(nameof(ServerSettings));

        return services
            .Configure<ServerSettings>(settingsSection);
    }
}
