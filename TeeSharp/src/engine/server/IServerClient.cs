using System;
using System.Collections.Generic;
using System.Text;

namespace TeeSharp.Server
{
    public enum SnapRate
    {
        INIT = 0,
        FULL,
        RECOVER
    }

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
        ConsoleAccessLevel AccessLevel { get; set; }
        ServerClientState ClientState { get; set; }
        string Name { get; set; }
        string Clan { get; set; }
        int Country { get; set; }
        int AuthTries { get; set; }
        long Traffic { get; set; }
        long TrafficSince { get; set; }

        SnapshotStorage SnapshotStorage { get; }

        void Reset();
    }
}
