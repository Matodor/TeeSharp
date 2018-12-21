using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvEmoticon : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerEmoticon;

        public int ClientId { get; set; }
        public Emoticons Emoticon { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(ClientId);
            packer.AddInt((int) Emoticon);
            return packer.Error;
        }
    }
}