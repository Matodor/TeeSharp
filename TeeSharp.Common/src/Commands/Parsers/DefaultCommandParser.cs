namespace TeeSharp.Common.Commands.Parsers
{
    public class DefaultCommandParser : ICommandParser
    {
        public (bool ok, string cmd, string args) Parse(string line)
        {
            if (!Valid(line))
                return (false, null, null);

            line = line.TrimStart();
            var space = line.IndexOf(' ');
            
            var cmd = space > 0 ? line[..space] : line;

            var args = string.Empty;
            if (space > 0 && space + 1 < line.Length)
                args = line[(space + 1)..];
            
            return (true, cmd, args);
        }

        private static bool Valid(string line)
        {
            return !string.IsNullOrWhiteSpace(line);
        }
    }
}