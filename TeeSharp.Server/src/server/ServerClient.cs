using TeeSharp.Common.Protocol;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Server
{
    public class ServerClient : BaseServerClient
    {
        public ServerClient()
        {
            State = ServerClientState.Empty;
            SnapshotStorage = new SnapshotStorage();
            Inputs = new Input[MaxInputs];

            for (var i = 0; i < Inputs.Length; i++)
            {
                Inputs[i] = new Input()
                {
                    Tick = -1,
                    PlayerInput = null
                };
            }
        }

        public override void Reset()
        {
            for (var i = 0; i < Inputs.Length; i++)
            {
                Inputs[i].Tick = -1;
            }

            CurrentInput = 0;
            LatestInput = null;

            SnapshotStorage.PurgeAll();
            LastAckedSnapshot = -1;
            LastInputTick = -1;
            SnapshotRate = SnapshotRate.Init;
            MapChunk = 0;
        }
    }
}