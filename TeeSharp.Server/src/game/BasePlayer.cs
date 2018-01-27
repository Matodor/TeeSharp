using TeeSharp.Common.Enums;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public class TeeInfo
    {
        public string SkinName { get; set; }
        public bool UseCustomColor { get; set; }
        public int ColorBody { get; set; }
        public int ColorFeet { get; set; }
    }

    public abstract class BasePlayer : BaseInterface
    {
        public abstract int ClientId { get; }
        public abstract bool IsReady { get; set; }
        public abstract long LastChangeInfo { get; set; }
        public abstract TeeInfo TeeInfo { get; protected set; }

        public abstract void Init(int clientId, Team team);
    }
}