using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
    public class GameControllerDM : VanillaController
    {
        public override string GameType { get; } = "DM";
    }
}