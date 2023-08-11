using System;
using System.Text.Json.Serialization;

namespace TeeSharp.MasterServer;

public class ServerInfoClient : IEquatable<ServerInfoClient>
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("clan")]
    public string Clan { get; init; } = string.Empty;

    [JsonPropertyName("country")]
    public int Country { get; init; }

    [JsonPropertyName("score")]
    public int Score { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("skin")]
    public ServerInfoClientSkin? Skin { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("team")]
    public int? Team { get; init; }

    [JsonPropertyName("is_player")]
    public bool IsPlayer { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("afk")]
    public bool? IsAfk { get; init; }

    public bool Equals(ServerInfoClient? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return
            Name == other.Name &&
            Clan == other.Clan &&
            Country == other.Country &&
            Score == other.Score &&
            IsPlayer == other.IsPlayer &&
            Equals(Skin, other.Skin) &&
            IsAfk == other.IsAfk &&
            Team == other.Team;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((ServerInfoClient)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Clan, Country, Score, IsPlayer, Skin, IsAfk, Team);
    }

    public ServerInfoClient Clone()
    {
        return new ServerInfoClient
        {
            Name = Name,
            Clan = Clan,
            Country = Country,
            Score = Score,
            Skin = Skin?.Clone(),
            Team = Team,
            IsPlayer = IsPlayer,
            IsAfk = IsAfk,
        };
    }
}
