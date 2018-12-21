using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClKill : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ClientKill;

        public override bool PackError(MsgPacker packer)
        {
            return packer.Error;
        }
    }
}