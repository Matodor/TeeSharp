namespace TeeSharp.Common.Commands.Builders
{
    public class ParameterBuilder
    {
        public ParameterBuilder()
        {
            
        }
        
        public ParameterInfo Build()
        {
            return new ParameterInfo(this);
        }
    }
}