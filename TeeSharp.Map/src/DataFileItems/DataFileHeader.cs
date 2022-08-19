using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TeeSharp.Map.DataFileItems;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DataFileHeader
{
    public unsafe Span<byte> Signature => new Span<byte>(Unsafe.AsPointer(ref _signature1), 4);

    public bool IsValidSignature =>
        Encoding.ASCII.GetString(Signature) == "DATA" ||
        Encoding.ASCII.GetString(Signature) == "ATAD";

    public bool IsValidVersion => Version == 4;

#pragma warning disable CS0169
    // ReSharper disable FieldCanBeMadeReadOnly.Local

    private byte _signature1;
    private byte _signature2;
    private byte _signature3;
    private byte _signature4;

    // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore CS0169

    public int Version;
    public int Size;
    public int SwapLength;
    public int NumberOfItemTypes;
    public int NumberOfItems;
    public int NumberOfRawDataBlocks;
    public int ItemsSize;
    public int RawDataBlocksSize;
}
