using TeeSharp.Common.NetObjects;
using TeeSharp.Common.Snapshots;
using TeeSharp.Core;

namespace TeeSharp.Server
{
    public enum ServerClientState
    {
        EMPTY = 0,
        AUTH,
        CONNECTING,
        READY,
        IN_GAME,
    }

    public enum SnapRate
    {
        INIT = 0,
        FULL,
        RECOVER
    }

    public abstract class BaseServerClient : BaseInterface
    {
        public const int 
            MAX_INPUT_SIZE = 128,
            INPUT_COUNT = 200;
            
        public class Input
        {
            public long Tick { get; set; }
            public NetObj_PlayerInput PlayerInput { get; set; }
        }

        public abstract SnapRate SnapRate { get; set; }
        public abstract ServerClientState State { get; set; }
        public abstract int Latency { get; set; }

        public abstract string PlayerName { get; set; }
        public abstract string PlayerClan { get; set; }
        public abstract int PlayerCountry { get; set; }

        public abstract long TrafficSince { get; set; }
        public abstract long Traffic { get; set; }

        public abstract long LastAckedSnapshot { get; set; }
        public abstract long LastInputTick { get; set; }
        public abstract int CurrentInput { get; set; }

        public abstract SnapshotStorage SnapshotStorage { get; protected set; }
        //public abstract Input LatestInput { get; protected set; }
        public abstract Input[] Inputs { get; protected set; }

        public abstract void Reset();
    }
}