using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClEmoticon : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_EMOTICON;

        public Emoticons Emoticon;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) Emoticon);
            return packer.Error;
        }
    }
}