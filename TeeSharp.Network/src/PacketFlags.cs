namespace TeeSharp.Network
{
    public enum PacketFlags
    {
        None     = 0b_0000_0000,
        Vital    = 0b_0000_0001,
        Connless = 0b_0000_0010,
        Flush    = 0b_0000_0100,
        Extended = 0b_0000_1000
    }
}