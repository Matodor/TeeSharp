using System;
using System.Security.Cryptography;

namespace TeeSharp.Common
{
    public static class Secure
    {
        public static readonly MD5 MD5Provider
            = MD5CryptoServiceProvider.Create();
        public static readonly RandomNumberGenerator RNGProvider 
            = RNGCryptoServiceProvider.Create();

        static Secure()
        {
            MD5Provider.Initialize();
        }

        public static uint RandomUInt32()
        {
            var bytes = new byte[4];
            RandomFill(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static void RandomFill(byte[] bytes)
        {
            RNGProvider.GetBytes(bytes);
        }
    }
}