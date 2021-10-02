using TeeSharp.Common.Commands.Builders;

namespace TeeSharp.Common.Commands
{
    public class ParameterInfo : IParameterInfo
    {
        public const char ParameterString = 's';
        public const char ParameterFloat = 'f';
        public const char ParameterInt = 'i';
        public const char ParameterRest = 'r';
        public const char ParameterOptional = '?';
        
        public bool IsOptional { get; }
        public bool IsRemain { get; }
        public IArgumentReader ArgumentReader { get; }
        
        internal ParameterInfo(ParameterBuilder builder)
        {
            
        }
    }
}
