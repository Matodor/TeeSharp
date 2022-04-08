namespace TeeSharp.Commands;

public interface IParameterInfo
{
    string Name { get; }
    string? Description { get; }
    bool IsOptional { get; }
    bool IsRemain { get; }
    IArgumentReader ArgumentReader { get; }
}
