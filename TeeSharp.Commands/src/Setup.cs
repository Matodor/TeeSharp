using Microsoft.Extensions.DependencyInjection;
using TeeSharp.Commands.Parsers;

namespace TeeSharp.Commands;

public static class Setup
{
    public static IServiceCollection AddCommands(
        this IServiceCollection services)
    {
        return services
            .AddTransient<ICommandArgumentsParser, DefaultCommandArgumentsParser>()
            .AddTransient<ICommandLineParser, DefaultCommandLineParser>()
            .AddTransient<ICommandsDictionary, CommandsDictionary>()
            .AddTransient<ICommandsExecutor, CommandsExecutor>();
    }
}
