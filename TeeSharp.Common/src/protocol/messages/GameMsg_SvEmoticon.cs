using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvEmoticon : BaseGameMessage, IClampedMaxClients
    {
        public override GameMessage Type => GameMessage.ServerEmoticon;

        public int ClientId { get; set; }
        public Emoticon Emoticon { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(ClientId);
            packer.AddInt((int) Emoticon);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            ClientId = unpacker.GetInt();
            Emoticon = (Emoticon) unpacker.GetInt();

            if (ClientId < 0)
                failedOn = nameof(ClientId);

            if (Emoticon < 0 || Emoticon >= Emoticon.NumEmoticons)
                failedOn = nameof(Emoticon);

            return unpacker.Error;
        }

        public void Validate(int maxClients, ref string failedOn)
        {
            if (ClientId < 0 || ClientId >= maxClients)
                failedOn = nameof(ClientId);
        }
    }
}