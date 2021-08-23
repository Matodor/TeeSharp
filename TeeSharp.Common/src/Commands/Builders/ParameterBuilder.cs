namespace TeeSharp.Common.Commands.Builders
{
    public class ParameterBuilder
    {
        internal ParameterInfo Build()
        {
            return new ParameterInfo(this);
        }
    }
}