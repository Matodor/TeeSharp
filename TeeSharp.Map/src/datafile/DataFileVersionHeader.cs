using System;
using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    /// <summary>
    /// 8 bytes
    /// The version header consists of a magic byte sequence, identifying the file as a Teeworlds datafile and a version number.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class DataFileVersionHeader
    {
        public string Magic => new string(MagicArray);

        /// <summary>
        /// 4 bytes
        /// The magic must exactly be the ASCII representations of the four characters, 
        /// 'D', 'A', 'T', 'A'. NOTE: Readers of Teeworlds datafiles should be able to read 
        /// datafiles which start with a reversed magic too, that is 'A', 'T', 'A', 'D'. 
        /// A bug in the reference implementation caused big-endian machines to save the reversed magic bytes.
        /// </summary>
        
        //[FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] MagicArray;

        /// <summary>
        /// 4 bytes
        /// The version is a little-endian signed 32-bit integer, for version 
        /// 3 or 4 of Teeworlds datafiles, it must be 3 or 4, respectively.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int Version;
    }
}