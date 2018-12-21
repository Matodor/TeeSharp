using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClEmoticon : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ClientEmoticon;

        public Emoticons Emoticon { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) Emoticon);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Emoticon = (Emoticons) unpacker.GetInt();

            if (Emoticon < 0 || Emoticon >= Emoticons.NumEmoticons)
                failedOn = nameof(Emoticon);

            return unpacker.Error;
        }
    }
}