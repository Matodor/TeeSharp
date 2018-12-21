using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvReadyToEnter : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerReadyToEnter;

        public override bool PackError(MsgPacker packer)
        {
            return packer.Error;
        }
    }
}