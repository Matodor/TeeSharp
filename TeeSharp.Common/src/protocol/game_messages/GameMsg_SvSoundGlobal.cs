using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvSoundGlobal : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_SOUNDGLOBAL;

        public Sound Sound;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt((int) Sound);
            return packer.Error;
        }
    }
}