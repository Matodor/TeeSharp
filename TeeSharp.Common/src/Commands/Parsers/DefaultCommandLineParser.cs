namespace TeeSharp.Common.Commands.Parsers
{
    public class DefaultCommandLineParser : ICommandLineParser
    {
        public string Prefix { get; set; } = "/";
        public bool TrimLine { get; set; } = true;

        public bool TryParse(string line, out string command, out string args, 
            out LineParseError? parseError)
        {
            if (TrimLine) 
                line = line?.Trim();

            if (!Valid(line, out var spaceIndex, out parseError))
            {
                command = null;
                args = null;
                return false;
            }
            
            command = spaceIndex < 0
                ? line.Substring(Prefix.Length)
                : line.Substring(Prefix.Length, spaceIndex - Prefix.Length);

            args = spaceIndex < 0
                ? null
                : line.Substring(spaceIndex);
            
            return true;
        }

        protected virtual bool Valid(string line, out int spaceIndex, 
            out LineParseError? error)
        {
            spaceIndex = -1;

            if (string.IsNullOrWhiteSpace(line))
            {
                error = LineParseError.EmptyLine;
                return false;
            }

            if (line.Length < Prefix.Length + CommandInfo.MinCommandLength)
            {
                error = LineParseError.BadLength;
                return false;
            }
            
            if (!line.StartsWith(Prefix))
            {
                error = LineParseError.WrongPrefix;
                return false;
            }

            spaceIndex = line.IndexOf(' ', Prefix.Length);

            if (spaceIndex < Prefix.Length + CommandInfo.MinCommandLength)
            {
                error = LineParseError.BadLength;
                spaceIndex = -1;
                return false;
            }

            error = null;
            return spaceIndex < 0;
        }
    }
}