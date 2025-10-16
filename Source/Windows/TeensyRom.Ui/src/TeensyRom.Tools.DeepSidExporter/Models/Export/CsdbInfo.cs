using System.Text.Json.Serialization;

namespace TeensyRom.Tools.DeepSidExporter.Models.Export;

/// <summary>
/// Represents CSDB information for composers and files
/// </summary>
public class CsdbInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}