using System.Runtime.InteropServices;

namespace TeeSharp.MasterServer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ServerEndpoint
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] IpData;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] PortData;
    }
}
