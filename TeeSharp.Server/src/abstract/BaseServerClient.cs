using TeeSharp.Common.Protocol;
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
            public int Tick { get; set; }
            public SnapshotPlayerInput PlayerInput { get; set; }
        }

        public virtual SnapRate SnapRate { get; set; }
        public virtual ServerClientState State { get; set; }
        public virtual int Latency { get; set; }

        public virtual string PlayerName { get; set; }
        public virtual string PlayerClan { get; set; }
        public virtual int PlayerCountry { get; set; }

        public virtual long TrafficSince { get; set; }
        public virtual int Traffic { get; set; }

        public virtual int LastAckedSnapshot { get; set; }
        public virtual int LastInputTick { get; set; }
        public virtual int CurrentInput { get; set; }

        public virtual SnapshotStorage SnapshotStorage { get; protected set; }
        public virtual Input[] Inputs { get; protected set; }

        public abstract void Reset();
    }
}