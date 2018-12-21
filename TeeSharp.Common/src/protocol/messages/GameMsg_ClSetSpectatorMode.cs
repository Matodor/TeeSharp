using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClSetSpectatorMode : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ClientSetSpectatorMode;

        public SpectatorMode SpectatorMode { get; set; }
        public int SpectatorId { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) SpectatorMode);
            packer.AddInt(SpectatorId);
            return packer.Error;
        }
    }
}