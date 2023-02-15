using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TeeSharp.MasterServer;

public class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("game_type")]
    public string GameType { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("max_clients")]
    public int MaxClients { get; set; }

    [JsonPropertyName("max_players")]
    public int MaxPlayers { get; set; }

    [JsonPropertyName("passworded")]
    public bool HasPassword { get; set; }

    [JsonPropertyName("map")]
    public ServerInfoMap Map { get; set; } = null!;

    [JsonPropertyName("clients")]
    public IEnumerable<ServerInfoClient> Clients { get; set; } = ArraySegment<ServerInfoClient>.Empty;
}
