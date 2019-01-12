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
            LatestInput = new Input();
            
            for (var i = 0; i < Inputs.Length; i++)
            {
                Inputs[i] = new Input()
                {
                    Tick = -1,
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
            LatestInput.Tick = 0;
            for (var i = 0; i < LatestInput.Data.Length; i++)
                LatestInput.Data[i] = 0;

            SnapshotStorage.PurgeAll();
            LastAckedSnapshot = -1;
            LastInputTick = -1;
            SnapshotRate = SnapshotRate.Init;
            MapChunk = 0;
            AuthTries = 0;
            AuthLevel = 0;

            if (SendCommandsEnumerator != null)
            {
                SendCommandsEnumerator.Dispose();
                SendCommandsEnumerator = null;
            }
        }
    }
}