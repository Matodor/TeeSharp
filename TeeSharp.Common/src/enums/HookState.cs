namespace TeeSharp.Common.Enums
{
    public enum HookState
    {
        RETRACTED = -1,
        IDLE = 0,
        RETRACT_START = 1,
        RETRACT_PROCESS = 2,
        RETRACT_END = 3,
        FLYING = 4,
        GRABBED = 5, 
    }
}