namespace TeeSharp.Common
{
    public class MsgPacker : Packer
    {
        public MsgPacker(int msgId)
        {
            Reset();
            AddInt(msgId);
        }
    }
}