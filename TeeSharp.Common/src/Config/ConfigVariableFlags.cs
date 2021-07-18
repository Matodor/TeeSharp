using System;

namespace TeeSharp.Common.Config
{
    [Flags]
    public enum ConfigVariableFlags
    {
        None = 0,
        
        Save = 1 << 0,
        Client = 1 << 1,
        Server = 1 << 2,
        Store = 1 << 3,
        Master = 1 << 4,
        Econ = 1 << 5,

        All = Save | Client | Server | Store | Master | Econ
    }
}