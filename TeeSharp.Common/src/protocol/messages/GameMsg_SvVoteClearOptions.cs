using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteClearOptions : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerVoteClearOptions;

        public override bool PackError(MsgPacker packer)
        {
            return packer.Error;
        }
    }
}