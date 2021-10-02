namespace TeeSharp.Common.Commands.Builders
{
    public class ParameterBuilder : IParameterInfo
    {
        public bool IsOptional { get; protected set; } = false;
        public bool IsRemain { get; protected set; }
        public IArgumentReader ArgumentReader { get; protected set; }
        
        public ParameterBuilder()
        {
        }

        public ParameterBuilder WithReader<T>() where T : IArgumentReader, new()
        {
            ArgumentReader = ReadersContainer.GetInstance<T>();
            return this;
        }
        
        public ParameterBuilder WithOptional(bool isOptional)
        {
            IsOptional = isOptional;
            return this;
        }
        
        public ParameterBuilder WithRemain(bool isRemain)
        {
            IsRemain = isRemain;
            return this;
        }
        
        public ParameterInfo Build()
        {
            return new ParameterInfo(this);
        }
    }
}
