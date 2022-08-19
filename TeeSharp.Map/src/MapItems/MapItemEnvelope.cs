using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TeeSharp.Core.Extensions;
using TeeSharp.Map.Abstract;

namespace TeeSharp.Map.MapItems;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MapItemEnvelope : IDataFileItem
{
    public const int CurrentVersion = 2;

    public unsafe Span<int> NameBuffer => new(Unsafe.AsPointer(ref _nameData1), 8);

    public string Name
    {
        get => NameBuffer.GetString();
        set => NameBuffer.PutString(value);
    }

    public bool IsSynchronized
    {
        get => _isSynchronized == 1;
        set => _isSynchronized = value ? 1 : 0;
    }

    public int ItemVersion;
    public int Channels;
    public int StartPoint;
    public int NumberOfPoints;

#pragma warning disable CS0169
    // ReSharper disable FieldCanBeMadeReadOnly.Local

    private int _nameData1;
    private int _nameData2;
    private int _nameData3;
    private int _nameData4;
    private int _nameData5;
    private int _nameData6;
    private int _nameData7;
    private int _nameData8;
    private int _isSynchronized;

    // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore CS0169
}
