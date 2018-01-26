using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvSoundGlobal : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_SOUNDGLOBAL;

        public Sounds Sound { get; set; }

        public override bool Pack(MsgPacker packer)
        {
            packer.AddInt((int) Sound);
            return packer.Error;
        }
    }
}