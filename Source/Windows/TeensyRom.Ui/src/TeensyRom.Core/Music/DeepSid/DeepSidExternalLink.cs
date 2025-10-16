using System.Text.Json.Serialization;

namespace TeensyRom.Core.Music.DeepSid;

/// <summary>
/// Represents an external link for a composer
/// </summary>
public class DeepSidExternalLink
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
