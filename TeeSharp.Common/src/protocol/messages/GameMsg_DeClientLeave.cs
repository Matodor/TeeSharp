using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_DeClientLeave : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.DemoClientLeave;

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

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            Name = unpacker.GetString(Sanitize);
            ClientId = unpacker.GetInt();
            Reason = unpacker.GetString(Sanitize);

            return unpacker.Error;
        }
    }
}