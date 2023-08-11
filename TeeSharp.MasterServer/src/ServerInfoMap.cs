using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeeSharp.MasterServer;

public class ServerInfoMap : IEquatable<ServerInfoMap>
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Checksum { get; init; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; init; } = 0;

    public bool Equals(ServerInfoMap? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return
            Name == other.Name &&
            Checksum == other.Checksum &&
            Size == other.Size;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((ServerInfoMap)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Checksum, Size);
    }

    public ServerInfoMap Clone()
    {
        return new ServerInfoMap
        {
            Name = Name,
            Checksum = Checksum,
            Size = Size,
        };
    }
}
