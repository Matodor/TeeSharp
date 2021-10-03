namespace TeeSharp.Commands
{
    public interface IParameterInfo
    {
        public bool IsOptional { get; }
        public bool IsRemain { get; }
        public IArgumentReader ArgumentReader { get; }
    }
}
