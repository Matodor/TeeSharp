using System;
using System.Runtime.CompilerServices;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Map;

public struct MapItemEnvelope : IDataFileItem
{
    public const int CurrentVersion = 2;
        
    public unsafe Span<int> NameBuffer 
        => new Span<int>(Unsafe.AsPointer(ref _nameData[0]), 8);

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
    public int PointsCount;
    private unsafe fixed int _nameData[8];
    private int _isSynchronized;
}