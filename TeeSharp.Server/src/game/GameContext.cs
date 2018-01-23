using TeeSharp.Common;
using TeeSharp.Common.Enums;

namespace TeeSharp.Server.Game
{
    public class GameContext : BaseGameContext
    {
        public override void RegisterCommands()
        {
        }

        public override void OnInit()
        {
        }

        public override void OnTick()
        {
        }

        public override void OnClientPredictedInput(int clientId, int[] data)
        {
        }

        public override void OnShutdown()
        {
            throw new System.NotImplementedException();
        }

        public override void OnMessage(NetworkMessages message, Unpacker unpacker, int clientId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnBeforeSnapshot()
        {
            throw new System.NotImplementedException();
        }

        public override void OnAfterSnapshot()
        {
            throw new System.NotImplementedException();
        }

        public override void OnSnapshot(int clientId)
        {
            throw new System.NotImplementedException();
        }
    }
}