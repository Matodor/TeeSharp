using System.Collections.Generic;

namespace TeeSharp.Commands;

public interface ICommandInfo
{
    public IReadOnlyList<IParameterInfo> Parameters { get; }
    public CommandHandler Callback { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
}
