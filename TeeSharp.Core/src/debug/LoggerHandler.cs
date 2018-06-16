using System;
using System.IO;
using System.Text;

namespace TeeSharp.Core
{
    public class LoggerHandler : ILoggerHandler
    {
        public TextWriter TextWriter { get; set; }

        public LoggerHandler()
        {
            Console.OutputEncoding = Encoding.UTF8;
            TextWriter = System.Console.Out;
        }

        public void Write(string format)
        {
            TextWriter.Write(format);
        }

        public void WriteLine(string format)
        {
            TextWriter.WriteLine(format);
        }
    }
}