using TeeSharp.Common;
using TeeSharp.Common.Snapshots;

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

    public abstract class BaseServerClient : BaseInterface
    {
        public class Input
        {
            public readonly int[] Data;
            public readonly long Tick;

            public Input(int[] data, long tick)
            {
                Data = data;
                Tick = tick;
            }
        }

        public abstract ServerClientState State { get; set; }
        public abstract SnapshotStorage SnapshotStorage { get; protected set; }
        public abstract Input[] Inputs { get; protected set; }
    }
}