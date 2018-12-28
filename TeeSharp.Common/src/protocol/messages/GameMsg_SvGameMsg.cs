using TeeSharp.Common.Enums;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvGameMsg : BaseGameMessage
    {
        public override GameMessage Type => GameMessage.ServerGameMessage;

        public GameplayMessage Message { get; set; }
        public int? Param1 { get; set; }
        public int? Param2 { get; set; }
        public int? Param3 { get; set; }

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) Message);
            if (Param1.HasValue)
                packer.AddInt(Param1.Value);
            if (Param2.HasValue)
                packer.AddInt(Param2.Value);
            if (Param3.HasValue)
                packer.AddInt(Param3.Value);
            return packer.Error;
        }

        public override bool UnPackError(UnPacker unpacker, ref string failedOn)
        {
            return unpacker.Error;
        }
    }
}