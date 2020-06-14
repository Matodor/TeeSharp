using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_ClEmoticon : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ClientEmoticon;

        public Emoticon Emoticon { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) Emoticon);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Emoticon = (Emoticon) unpacker.GetInt();

            if (Emoticon < 0 || Emoticon >= Emoticon.NumEmoticons)
                failedOn = nameof(Emoticon);

            return unpacker.Error;
        }
    }
}