using TeeSharp.Commands.Builders;

namespace TeeSharp.Commands;

public class ParameterInfo : IParameterInfo
{
    public string Name { get; }
    public string? Description { get; }
    public bool IsOptional { get; }
    public bool IsRemain { get; }
    public IArgumentReader ArgumentReader { get; }

    internal ParameterInfo(ParameterBuilder builder)
    {
        Name = builder.Name!;
        Description = builder.Description;
        IsOptional = builder.IsOptional;
        IsRemain = builder.IsRemain;
        ArgumentReader = builder.ArgumentReader;
    }
}
