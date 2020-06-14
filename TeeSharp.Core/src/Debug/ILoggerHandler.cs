using System.IO;

namespace TeeSharp.Core
{
    public interface ILoggerHandler
    {
        TextWriter TextWriter { get; set; }

        void Write(string format);
        void WriteLine(string format);
    }
}