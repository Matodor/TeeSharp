using TeeSharp.Network;

namespace TeeSharp.Common
{
    public class MsgPacker : Packer
    {
        public MsgPacker(int msgId, bool system)
        {
            Reset();
            AddInt((msgId << 1) | (system ? 1 : 0));
        }
    }
}