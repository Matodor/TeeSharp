using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvClientDrop : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerClientDrop;

        public int ClientID { get; set; }
        public string Reason { get; set; }
        public bool Silent { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(ClientID);
            packer.AddString(Reason);
            packer.AddBool(Silent);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            ClientID = unpacker.GetInt();
            Reason = unpacker.GetString(Sanitize);
            Silent = unpacker.GetBool();

            if (ClientID < 0)
                failedOn = nameof(ClientID);

            return unpacker.Error;
        }
    }
}