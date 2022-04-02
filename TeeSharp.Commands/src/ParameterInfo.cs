using TeeSharp.Commands.Builders;

namespace TeeSharp.Commands;

public class ParameterInfo : IParameterInfo
{
    public bool IsOptional { get; }
    public bool IsRemain { get; }
    public IArgumentReader ArgumentReader { get; }
        
    internal ParameterInfo(ParameterBuilder builder)
    {
        IsOptional = builder.IsOptional;
        IsRemain = builder.IsRemain;
        ArgumentReader = builder.ArgumentReader;
    }
}
