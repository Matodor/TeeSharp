using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvTuneParams : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerTuneParams;

        public override bool PackError(MsgPacker packer)
        {
            return packer.Error;
        }
    }
}