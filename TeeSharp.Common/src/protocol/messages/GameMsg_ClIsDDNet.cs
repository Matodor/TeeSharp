using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClIsDDNet : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_ISDDNET;

        public override bool PackError(MsgPacker packer)
        {
            return packer.Error;
        }
    }
}