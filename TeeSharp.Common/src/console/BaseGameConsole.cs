namespace TeeSharp.Common.Console
{
    public enum OutputLevel
    {
        STANDARD = 0,
        ADDINFO = 1,
        DEBUG = 2,
    }

    public abstract class BaseGameConsole : BaseInterface
    {
        public abstract void Init();
        public abstract void ExecuteFile(string fileName, bool forcibly = false);
        public abstract void ParseArguments(string[] args);
        public abstract void ExecuteLine(string line);
        public abstract void Print(OutputLevel outputLevel, string sys, string format);
    }
}