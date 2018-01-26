namespace TeeSharp.Server.Game
{
    public class Player : BasePlayer
    {
        public override int ClientId => _clientId;

        private int _clientId;

        public override void Init(int clientId)
        {
            _clientId = clientId;
        }
    }
}