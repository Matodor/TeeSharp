using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_DeClientEnter : BaseGameMessage
    {
        public override GameMessages Type => GameMessages.DemoClientEnter;

        public string Name { get; set; }
        public int ClientId { get; set; }
        public Team Team { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddString(Name);
            packer.AddInt(ClientId);
            packer.AddInt((int) Team);
            return packer.Error;
        }
    }
}