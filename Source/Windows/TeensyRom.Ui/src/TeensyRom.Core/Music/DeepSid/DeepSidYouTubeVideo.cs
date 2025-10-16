using System.Text.Json.Serialization;

namespace TeensyRom.Core.Music.DeepSid;

/// <summary>
/// Represents a YouTube video reference
/// </summary>
public class DeepSidYouTubeVideo
{
    [JsonPropertyName("video_id")]
    public string VideoId { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonPropertyName("subtune")]
    public int Subtune { get; set; }
}
