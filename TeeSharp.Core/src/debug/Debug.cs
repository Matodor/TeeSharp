using System;

namespace TeeSharp.Core
{
    public static class Debug
    {
        public static ILogger Logger { get; set; } = new Logger();

        public static void Assert(bool condition, string error)
        {
            if (!condition)
                Assert("assert", error);
        }

        public static void Log(string sys, string format)
        {
            Logger.LogFormat(LogType.Log, $"[{sys}] {format}");
        }

        public static void Error(string sys, string format)
        {
            Logger.LogFormat(LogType.Error, $"[{sys}] {format}");
        }

        public static void Assert(string sys, string format)
        {
            Logger.LogFormat(LogType.Assert, $"[{sys}] {format}");
        }

        public static void Warning(string sys, string format)
        {
            Logger.LogFormat(LogType.Warning, $"[{sys}] {format}");
        }

        public static void Exception(string sys, Exception exception)
        {
            Logger.LogFormat(LogType.Exception, $"[{sys}] {exception}");
        }

        public static void Exception(string sys, string format)
        {
            Logger.LogFormat(LogType.Exception, $"[{sys}] {format}");
        }
    }
}