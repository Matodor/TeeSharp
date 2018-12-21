using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_DeClientLeave : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.DemoClientLeave;

        public string Name { get; set; }
        public int ClientId { get; set; }
        public string Reason { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Name);
            packer.AddInt(ClientId);
            packer.AddString(Reason);
            return packer.Error;
        }
    }
}