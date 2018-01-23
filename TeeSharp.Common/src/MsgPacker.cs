using TeeSharp.Common.Enums;

namespace TeeSharp.Common
{
    public class MsgPacker : Packer
    {
        public MsgPacker(NetworkMessages type)
        {
            Reset();
            AddInt((int) type);
        }
    }
}