using System;
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

        public static void RandomFill(ushort[] array)
        {
            var buffer = new byte[array.Length * sizeof(ushort)];
            RandomFill(buffer);
            Buffer.BlockCopy(buffer, 0, array, 0, buffer.Length);
        }

        public static void RandomFill(byte[] bytes)
        {
            RNG.GetBytes(bytes);
        }
    }
}