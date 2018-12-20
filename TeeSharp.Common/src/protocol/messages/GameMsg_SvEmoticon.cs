using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvEmoticon : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_EMOTICON;

        public int ClientId;
        public Emoticons Emoticon;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(ClientId);
            packer.AddInt((int) Emoticon);
            return packer.Error;
        }
    }
}