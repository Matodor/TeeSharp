namespace TeeSharp.Common.Commands.Parsers
{
    public interface ICommandParser
    {
        (bool ok, string cmd, string args) Parse(string line);
    }
}