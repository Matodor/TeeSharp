using System.Text.Json.Serialization;

namespace TeeSharp.MasterServer;

public class ServerInfoMap
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Checksum { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; } = 0;
}
