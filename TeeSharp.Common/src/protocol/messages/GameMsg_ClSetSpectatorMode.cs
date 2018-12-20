using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClSetSpectatorMode : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_SETSPECTATORMODE;

        public int SpectatorId;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(SpectatorId);
            return packer.Error;
        }
    }
}