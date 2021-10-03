namespace TeeSharp.Common.Commands
{
    public interface IArgumentReader
    {
        public bool TryRead(string arg, out object value);
    }
}
