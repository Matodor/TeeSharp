using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClEmoticon : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ClientEmoticon;

        public Emoticons Emoticon { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) Emoticon);
            return packer.Error;
        }
    }
}