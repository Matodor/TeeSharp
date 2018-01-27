using TeeSharp.Common.Enums;

namespace TeeSharp.Server.Game
{
    public class Player : BasePlayer
    {
        public override int ClientId => _clientId;
        public override bool IsReady { get; set; }
        public override long LastChangeInfo { get; set; }
        public override TeeInfo TeeInfo { get; protected set; }

        private int _clientId;

        public override void Init(int clientId, Team team)
        {
            _clientId = clientId;

            TeeInfo = new TeeInfo();
            IsReady = false;
            LastChangeInfo = -1;
        }
    }
}