using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TeeSharp.MasterServer;

public class ServerInfo : IEquatable<ServerInfo>
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("game_type")]
    public string GameType { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    [JsonPropertyName("max_clients")]
    public int MaxClients { get; init; }

    [JsonPropertyName("max_players")]
    public int MaxPlayers { get; init; }

    [JsonPropertyName("passworded")]
    public bool HasPassword { get; init; }

    [JsonPropertyName("map")]
    public ServerInfoMap Map { get; init; } = new();

    [JsonPropertyName("clients")]
    public IEnumerable<ServerInfoClient> Clients { get; init; } = Array.Empty<ServerInfoClient>();

    public bool Equals(ServerInfo? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return
            Name == other.Name &&
            GameType == other.GameType &&
            Version == other.Version &&
            MaxClients == other.MaxClients &&
            MaxPlayers == other.MaxPlayers &&
            HasPassword == other.HasPassword &&
            Map.Equals(other.Map) &&
            Clients.SequenceEqual(other.Clients);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((ServerInfo)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Name,
            GameType,
            Version,
            MaxClients,
            MaxPlayers,
            HasPassword,
            Map,
            Clients
        );
    }

    public ServerInfo Clone()
    {
        return new ServerInfo
        {
            Name = Name,
            GameType = GameType,
            Version = Version,
            MaxClients = MaxClients,
            MaxPlayers = MaxPlayers,
            HasPassword = HasPassword,
            Map = Map.Clone(),
            Clients = new List<ServerInfoClient>(Clients.Select(client => client.Clone())),
        };
    }
}
