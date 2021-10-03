using TeeSharp.Common.Commands.Builders;

namespace TeeSharp.Common.Commands
{
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
}
