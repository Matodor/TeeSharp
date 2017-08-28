using System;
using System.Collections.Generic;
using System.Text;

namespace TeeSharp.Server
{
    public enum ServerClientState
    {
        EMPTY = 0,
        AUTH,
        CONNECTING,
        READY,
        INGAME,
    }

    public interface IServerClient
    {
        ServerClientState ClientState { get; set; }
    }
}
