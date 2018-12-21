using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteOptionListAdd : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerVoteOptionListAdd;

        public override bool PackError(MsgPacker packer)
        {
            return packer.Error;
        }
    }
}