using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteOptionRemove : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerVoteOptionRemove;

        public string Description { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Description);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Description = unpacker.GetString(Sanitize);
            return unpacker.Error;
        }
    }
}