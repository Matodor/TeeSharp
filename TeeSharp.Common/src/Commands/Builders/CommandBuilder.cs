namespace TeeSharp.Common.Commands.Builders
{
    public class CommandBuilder
    {
        internal CommandInfo Build()
        {
            return new CommandInfo(this);
        }
    }
}