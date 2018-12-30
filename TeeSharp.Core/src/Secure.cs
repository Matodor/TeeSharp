using System.Security.Cryptography;

namespace TeeSharp.Core
{
    public static class Secure
    {
        public static readonly MD5 MD5
            = MD5CryptoServiceProvider.Create();
        public static readonly RandomNumberGenerator RNG 
            = RNGCryptoServiceProvider.Create();

        static Secure()
        {
            MD5.Initialize();
        }

        //public static uint RandomUInt32()
        //{
        //    var bytes = new byte[4];
        //    RandomFill(bytes);
        //    return BitConverter.ToUInt32(bytes, 0);
        //}

        //public static void RandomFill(byte[] bytes)
        //{
        //    RNG.GetBytes(bytes);
        //}
    }
}