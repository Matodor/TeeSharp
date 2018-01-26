using TeeSharp.Common;

namespace TeeSharp.Server.Game.gamemodes
{
    public abstract class VanillaController : BaseGameController
    {
        public override void OnEntity(int entityIndex, Vector2 pos)
        {
            var entity = (MapItems) entityIndex;
        }
    }
}