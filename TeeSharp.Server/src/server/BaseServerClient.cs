using TeeSharp.Common;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Server
{
    public abstract class BaseServerClient : BaseInterface
    {
        public abstract SnapshotStorage SnapshotStorage { get; protected set; }
    }
}