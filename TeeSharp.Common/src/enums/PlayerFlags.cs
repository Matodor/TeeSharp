using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum PlayerFlags
    {
        None = 0,
        Admin = 1 << 0,
        Chatting = 1 << 1,
        Scoreboard = 1 << 2,
        Ready = 1 << 3,
        Dead = 1 << 4,
        Watching = 1 << 5,
        Bot = 1 << 6,

        All =
            None |
            Admin | 
            Chatting | 
            Scoreboard | 
            Ready | 
            Dead | 
            Watching | 
            Bot,
    }
}