using System.Text.Json.Serialization;

namespace TeensyRom.Core.Music.DeepSid;

/// <summary>
/// Represents CSDB (C64 Scene Database) information
/// </summary>
public class DeepSidCsdbInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
