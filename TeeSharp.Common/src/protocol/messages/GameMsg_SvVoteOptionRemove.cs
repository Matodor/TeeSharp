using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteOptionRemove : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerVoteOptionRemove;

        public string Description { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Description);
            return packer.Error;
        }
    }
}