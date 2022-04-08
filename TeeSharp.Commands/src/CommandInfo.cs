using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeeSharp.Commands.Builders;

namespace TeeSharp.Commands;

public delegate Task CommandHandler(ICommandResult result);

public class CommandInfo : ICommandInfo
{
    public const int MinCommandLength = 1;
    public const int MaxCommandLength = 64;
    public const int MaxDescriptionLength = 256;
    public const int MaxParamsLength = 32;

    public IReadOnlyList<IParameterInfo> Parameters { get; }
    public CommandHandler Callback { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

    internal CommandInfo(CommandBuilder builder)
    {
        Parameters = builder.Parameters.Select(b => b.Build()).ToList();
        Callback = builder.Callback!;
        Name = builder.Name!;
        Description = builder.Description;
    }
}
