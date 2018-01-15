using TeeSharp.Common.Snapshots;

namespace TeeSharp.Server
{
    public class ServerClient : BaseServerClient
    {
        public sealed override SnapshotStorage SnapshotStorage { get; protected set; }

        public ServerClient()
        {
            SnapshotStorage = new SnapshotStorage();
        }
    }
}