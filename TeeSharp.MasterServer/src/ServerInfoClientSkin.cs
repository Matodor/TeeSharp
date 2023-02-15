using System.Text.Json.Serialization;

namespace TeeSharp.MasterServer;

public class ServerInfoClientSkin
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("color_body")]
    public int? ColorBody { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("color_feet")]
    public int? ColorFeet { get; set; }
}
