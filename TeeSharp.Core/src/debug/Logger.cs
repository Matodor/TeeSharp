using System;
using System.Text;

namespace TeeSharp.Core
{
    public class Logger : ILogger
    {
        public ILoggerHandler Handler { get; set; } = new LoggerHandler();
        public LogType FilterLogType { get; set; } = LogType.Log;
        public bool LogEnabled { get; set; } = true;

        public Logger()
        {
            
        }

        public void LogFormat(LogType logType, string format, params object[] args)
        {
            if (!IsLogTypeAllowed(logType))
                return;

            System.Console.ForegroundColor = System.ConsoleColor.DarkYellow;
            Handler.Write($"[{DateTime.Now:G}]");

            switch (logType)
            {
                case LogType.Error:
                    System.Console.ForegroundColor = System.ConsoleColor.Red;
                    break;
                case LogType.Assert:
                    System.Console.ForegroundColor = System.ConsoleColor.Green;
                    break;
                case LogType.Warning:
                    System.Console.ForegroundColor = System.ConsoleColor.Yellow;
                    break;
                case LogType.Log:
                    System.Console.ForegroundColor = System.ConsoleColor.Gray;
                    break;
                case LogType.Exception:
                    System.Console.ForegroundColor = System.ConsoleColor.Red;
                    break;
                default:
                    System.Console.ForegroundColor = System.ConsoleColor.Gray;
                    break;
            }

            Handler.WriteLine(args == null || args.Length == 0 ? format : string.Format(format, args));
            System.Console.ResetColor();
        }

        public void LogException(Exception exception)
        {
            System.Console.ForegroundColor = System.ConsoleColor.Yellow;
            Handler.Write($"[{DateTime.Now:G}]");
            System.Console.ForegroundColor = System.ConsoleColor.Red;
            Handler.WriteLine(exception.ToString());
            System.Console.ResetColor();
        }

        public bool IsLogTypeAllowed(LogType logType)
        {
            if (!LogEnabled)
                return false;
            if (logType == LogType.Exception)
                return true;
            if (FilterLogType != LogType.Exception)
                return logType <= FilterLogType;
            return false;
        }
    }
}