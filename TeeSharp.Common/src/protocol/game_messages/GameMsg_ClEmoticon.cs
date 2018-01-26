using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClEmoticon : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.CL_EMOTICON;

        public Emoticons Emoticon { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddInt((int) Emoticon);
            return packer.Error;
        }
    }
}