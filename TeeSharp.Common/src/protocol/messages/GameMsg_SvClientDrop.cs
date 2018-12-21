using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvClientDrop : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.ServerClientDrop;

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
    }
}