using TeeSharp.Common;
using TeeSharp.Common.Enums;

namespace TeeSharp.Server.Game
{
    public abstract class VanillaController : BaseGameController
    {
        public override void OnEntity(int entityIndex, Vector2 pos)
        {
            var entity = (MapItems) entityIndex;
        }

        public override Team GetAutoTeam(int clientId)
        {
            throw new System.NotImplementedException();
        }

        public override bool CheckTeamsBalance()
        {
            throw new System.NotImplementedException();
        }
    }
}