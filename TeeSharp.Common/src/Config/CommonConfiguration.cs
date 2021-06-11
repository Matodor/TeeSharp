namespace TeeSharp.Common.Config
{
    public class CommonConfiguration : Configuration
    {
        public virtual ConfigVariableBoolean Debug { get; } = new ConfigVariableBoolean(
            defaultValue: false,
            description: "Debug"
        );
    }
}