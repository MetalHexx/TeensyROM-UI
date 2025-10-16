using System.Text.Json.Serialization;

namespace TeensyRom.Tools.DeepSidExporter.Models.Export;

/// <summary>
/// Represents a YouTube video associated with a file
/// </summary>
public class YoutubeVideo
{
    [JsonPropertyName("video_id")]
    public string VideoId { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("subtune")]
    public int Subtune { get; set; }

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;
}