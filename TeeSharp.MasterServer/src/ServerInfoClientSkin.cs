using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeeSharp.MasterServer;

public class ServerInfoClientSkin : IEquatable<ServerInfoClientSkin>
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("color_body")]
    public int? ColorBody { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("color_feet")]
    public int? ColorFeet { get; set; }

    public bool Equals(ServerInfoClientSkin? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return
            Name == other.Name &&
            ColorBody == other.ColorBody &&
            ColorFeet == other.ColorFeet;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((ServerInfoClientSkin)obj);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, ColorBody, ColorFeet);
    }

    public ServerInfoClientSkin Clone()
    {
        return new ServerInfoClientSkin
        {
            Name = Name,
            ColorBody = ColorBody,
            ColorFeet = ColorFeet,
        };
    }
}
