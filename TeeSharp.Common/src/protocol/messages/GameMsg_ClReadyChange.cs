using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClReadyChange : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ClientReadyChange;

        public override bool PackError(MsgPacker packer)
        {
            return packer.Error;
        }
    }
}