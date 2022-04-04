namespace TeeSharp.Commands;

public interface IParameterInfo
{
    public string Name { get; }
    public string? Description { get; }
    public bool IsOptional { get; }
    public bool IsRemain { get; }
    public IArgumentReader ArgumentReader { get; }
}
