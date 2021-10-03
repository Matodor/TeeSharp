namespace TeeSharp.Common.Commands.ArgumentsReaders
{
    public class StringReader : IArgumentReader
    {
        public bool TryRead(string arg, out object value)
        {
            value = arg;
            return true;
        }
    }
}
