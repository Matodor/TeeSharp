using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvEmoticon : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerEmoticon;

        public int ClientId { get; set; }
        public Emoticons Emoticon { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(ClientId);
            packer.AddInt((int) Emoticon);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            ClientId = unpacker.GetInt();
            Emoticon = (Emoticons) unpacker.GetInt();

            if (ClientId < 0)
                failedOn = nameof(ClientId);

            if (Emoticon < 0 || Emoticon >= Emoticons.NumEmoticons)
                failedOn = nameof(Emoticon);

            return unpacker.Error;
        }
    }
}