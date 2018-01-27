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
        public virtual bool IsReady { get; set; }
        public virtual long LastChangeInfo { get; set; }
        public virtual TeeInfo TeeInfo { get; protected set; }

        public abstract void Init(int clientId, Team team);
    }
}