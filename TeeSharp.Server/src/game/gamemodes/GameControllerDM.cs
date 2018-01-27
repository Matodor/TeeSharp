using TeeSharp.Common;
using TeeSharp.Common.Enums;

namespace TeeSharp.Server.Game
{
    public class GameControllerDM : VanillaController
    {
        public override string GameType { get; } = "DM";
    }
}