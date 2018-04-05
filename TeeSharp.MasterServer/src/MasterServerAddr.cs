using System.Runtime.InteropServices;

namespace TeeSharp.MasterServer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MasterServerAddr
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Ip;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] Port;
    }
}