using System.Text.Json.Serialization;

namespace TeeSharp.MasterServer;

public class ServerInfoClient
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("clan")]
    public string Clan { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public int Country { get; set; } = 0;

    [JsonPropertyName("score")]
    public int Score { get; set; } = 0;

    [JsonPropertyName("is_player")]
    public bool IsPlayer { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("skin")]
    public ServerInfoClientSkin? Skin { get; set; } = null;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("afk")]
    public bool? IsAfk { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("team")]
    public int? Team { get; set; }
}
