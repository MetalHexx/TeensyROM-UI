using System.Text.Json.Serialization;

namespace TeensyRom.Tools.DeepSidExporter.Models.Export;

/// <summary>
/// Represents a composer's external link
/// </summary>
public class ComposerLink
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}