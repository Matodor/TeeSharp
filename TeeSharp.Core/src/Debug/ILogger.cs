using System;

namespace TeeSharp.Core
{
    public enum LogType
    {
        Error = 0,
        Assert,
        Warning,
        Log,
        Exception,
    }

    public interface ILogger
    {
        ILoggerHandler Handler { get; set; }
        LogType FilterLogType { get; set; }
        bool LogEnabled { get; set; }
        
        void LogFormat(LogType logType, string format, params object[] args);
        void LogException(Exception exception);
        bool IsLogTypeAllowed(LogType logType);
    }
}