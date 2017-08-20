namespace TeeSharp
{
    public class MsgPacker : Packer
    {
        public MsgPacker(NetMessages type)
        {
            Reset();
            AddInt((int) type);
        }
    }
}
