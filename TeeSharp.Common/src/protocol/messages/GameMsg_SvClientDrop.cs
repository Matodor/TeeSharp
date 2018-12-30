using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvClientDrop : BaseGameMessage, IClampedMaxClients
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

            return unpacker.Error;
        }

        public void Validate(int maxClients, ref string failedOn)
        {
            if (ClientID < 0 || ClientID >= maxClients)
                failedOn = nameof(ClientID);
        }
    }
}